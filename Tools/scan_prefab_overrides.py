# -*- coding: utf-8 -*-
r"""씬과 프리팹이 원본 프리팹의 배치를 덮어쓰는 곳을 찾는다.

    python Tools/scan_prefab_overrides.py                 # 값이 실제로 다른 것만
    python Tools/scan_prefab_overrides.py --all           # 값이 같은(중복) 것까지
    python Tools/scan_prefab_overrides.py --path "Assets/01. Scenes"

왜 필요한가
    유니티는 프리팹 인스턴스마다 바뀐 값을 오버라이드로 들고 있습니다. 프리팹을 고쳐도
    인스턴스가 그 속성을 덮고 있으면 게임 화면은 바뀌지 않습니다. 오버라이드는 파일 안에
    묻혀 있어 눈으로 찾기 어렵고, 여러 겹으로 쌓이면 어느 값이 이기는지 추적이 힘듭니다.

    실제로 홈 대사창은 프리팹(가운데 아래) → 상위 프리팹(왼쪽 아래 537,1) → 씬(가운데 0,19)
    세 겹이 쌓여 있었고, 씬 값이 이겨 캐릭터가 바닥에서 18픽셀 떠 있었습니다.

읽는 법
    "값이 다름"은 그 인스턴스가 프리팹과 실제로 다르게 보이는 자리입니다. 의도한 것일 수도
    있고, 프리팹을 고쳤는데 반영되지 않는 원인일 수도 있습니다.

    "값이 같음"은 아무 효과가 없는 오버라이드입니다. 지워도 화면이 바뀌지 않고, 지우면
    프리팹을 고쳤을 때 인스턴스가 따라옵니다.

    중첩 프리팹은 한 단계씩 거슬러 올라가 원본 값을 찾습니다. 도중에 값을 덮는 층이 있으면
    그 층의 값을 원본으로 삼습니다. 실제로 게임에서 이기는 값을 기준으로 비교하기 위해서입니다.
"""
import argparse
import os
import re
import sys

# 배치에 관계된 것만 봅니다. 나머지 오버라이드(문구, 색 등)는 화면이 달라 보이는 원인을 찾는 데
# 도움이 되지 않으면서 결과만 크게 만듭니다.
LAYOUT_PROPERTIES = (
    "m_AnchoredPosition.x", "m_AnchoredPosition.y",
    "m_SizeDelta.x", "m_SizeDelta.y",
    "m_AnchorMin.x", "m_AnchorMin.y",
    "m_AnchorMax.x", "m_AnchorMax.y",
    "m_Pivot.x", "m_Pivot.y",
    "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z",
)

# 프리팹 루트의 트랜스폼은 유니티가 인스턴스마다 반드시 덮어씁니다. 원본에는 자리표시자만
# 들어 있어 비교하면 전부 "다름"으로 나오므로, 진짜 문제를 묻지 않도록 셈에서 뺍니다.

SEARCH_EXT = (".prefab", ".unity")

BLOCK_SPLIT = re.compile(r"(?m)^(?=--- )")
BLOCK_HEAD = re.compile(r"--- !u!(\d+) &(\d+)")
GUID_META = re.compile(r"^guid: ([0-9a-f]{32})\s*$", re.M)

MODIFICATION = re.compile(
    r"- target: \{fileID: (\d+), guid: ([0-9a-f]{32}), type: 3\}\n"
    r"      propertyPath: (\S+)\n"
    r"      value: (.*)\n")

SOURCE_PREFAB = re.compile(r"m_SourcePrefab: \{fileID: \d+, guid: ([0-9a-f]{32})")


def build_guid_index():
    """guid 로 에셋 경로를 찾을 수 있게 해 둡니다."""
    index = {}

    for dirpath, _, filenames in os.walk("Assets"):
        for filename in filenames:
            if not filename.endswith(".meta"):
                continue

            path = os.path.join(dirpath, filename)

            try:
                head = open(path, encoding="utf-8", errors="ignore").read(400)
            except OSError:
                continue

            found = GUID_META.search(head)

            if found:
                index[found.group(1)] = path[:-len(".meta")]

    return index


class Document(object):
    """프리팹이나 씬 파일 하나입니다. 블록을 fileID 로 찾을 수 있게 담아 둡니다."""

    def __init__(self, path):
        self.path = path
        self.blocks = {}
        self.instances = []

        try:
            raw = open(path, encoding="utf-8", errors="ignore").read().replace("\r\n", "\n")
        except OSError:
            return

        for block in BLOCK_SPLIT.split(raw):
            head = BLOCK_HEAD.match(block)

            if not head:
                continue

            self.blocks[head.group(2)] = block

            if head.group(1) == "1001":
                self.instances.append((head.group(2), block))

    def property_value(self, fid, prop):
        """이 파일 안에서 그 속성의 값을 읽습니다. 없으면 None 입니다."""
        block = self.blocks.get(fid)

        if block is None:
            return None

        field, _, axis = prop.partition(".")
        found = re.search(r"(?m)^  %s: \{(.*)\}$" % re.escape(field), block)

        if found:
            if not axis:
                return found.group(1).strip()

            axis_value = re.search(r"\b%s: ([-\d.eE]+)" % axis, found.group(1))
            return axis_value.group(1) if axis_value else None

        plain = re.search(r"(?m)^  %s: (.*)$" % re.escape(field), block)
        return plain.group(1).strip() if plain and not axis else None

    def object_name(self, fid):
        """RectTransform 이면 그것을 달고 있는 오브젝트의 이름을 돌려줍니다."""
        block = self.blocks.get(fid)

        if block is None:
            return None

        owner = re.search(r"m_GameObject: \{fileID: (\d+)\}", block)
        target = self.blocks.get(owner.group(1)) if owner else block

        if target is None:
            return None

        found = re.search(r"(?m)^  m_Name: (.*)$", target)
        return found.group(1).strip() if found else None

    def is_root_transform(self, fid):
        """프리팹 루트의 트랜스폼인지입니다.

        유니티는 인스턴스마다 루트의 위치·크기·스케일을 항상 오버라이드로 적습니다.
        원본에는 자리표시자만 들어 있어(스케일 0 등) 비교하면 늘 다르게 나옵니다.
        """
        block = self.blocks.get(fid)

        if block is None:
            return False

        return "m_Father: {fileID: 0}" in block

    def stripped_origin(self, fid):
        """중첩 프리팹에서 온 블록이면 (인스턴스 fileID, 원본 fileID) 를 돌려줍니다."""
        block = self.blocks.get(fid)

        if block is None or "stripped" not in block.split("\n")[0]:
            return None

        instance = re.search(r"m_PrefabInstance: \{fileID: (\d+)\}", block)
        origin = re.search(r"m_CorrespondingSourceObject: \{fileID: (\d+)", block)

        if not instance or not origin:
            return None

        return instance.group(1), origin.group(1)

    def instance_override(self, instance_fid, target_fid, prop):
        block = self.blocks.get(instance_fid)

        if block is None:
            return None

        for target, _, path, value in MODIFICATION.findall(block):
            if target == target_fid and path == prop:
                return value.strip()

        return None

    def instance_source_guid(self, instance_fid):
        block = self.blocks.get(instance_fid)

        if block is None:
            return None

        found = SOURCE_PREFAB.search(block)
        return found.group(1) if found else None


class Resolver(object):
    """원본 프리팹을 거슬러 올라가며 실제로 이기는 값을 찾습니다."""

    MAX_DEPTH = 8

    def __init__(self, guid_index):
        self.guid_index = guid_index
        self.cache = {}

    def document(self, path):
        if path not in self.cache:
            self.cache[path] = Document(path)

        return self.cache[path]

    def resolve(self, guid, fid, prop, depth=0):
        """그 프리팹 안에서 fid 의 prop 이 최종적으로 갖는 값입니다.

        (값, 중간 층이 정한 값인지) 를 함께 돌려줍니다. 뒤쪽 값이 필요한 이유는
        프리팹 루트 때문입니다. 루트의 원본 값은 자리표시자라 비교할 것이 못 되지만,
        중간 층이 자리를 정해 두었다면 그 값과는 비교할 수 있습니다.
        """
        if depth > self.MAX_DEPTH:
            return None, False

        path = self.guid_index.get(guid)

        if path is None or not os.path.exists(path):
            return None, False

        doc = self.document(path)
        origin = doc.stripped_origin(fid)

        if origin is None:
            return doc.property_value(fid, prop), False

        instance_fid, source_fid = origin
        override = doc.instance_override(instance_fid, source_fid, prop)

        if override is not None:
            return override, True

        inner_guid = doc.instance_source_guid(instance_fid)

        if inner_guid is None:
            return None, False

        return self.resolve(inner_guid, source_fid, prop, depth + 1)

    def is_root(self, guid, fid, depth=0):
        if depth > self.MAX_DEPTH:
            return False

        path = self.guid_index.get(guid)

        if path is None or not os.path.exists(path):
            return False

        doc = self.document(path)

        if doc.is_root_transform(fid):
            return True

        origin = doc.stripped_origin(fid)

        if origin is None:
            return False

        inner_guid = doc.instance_source_guid(origin[0])
        return self.is_root(inner_guid, origin[1], depth + 1) if inner_guid else False

    def name(self, guid, fid, depth=0):
        if depth > self.MAX_DEPTH:
            return None

        path = self.guid_index.get(guid)

        if path is None or not os.path.exists(path):
            return None

        doc = self.document(path)
        found = doc.object_name(fid)

        if found:
            return found

        origin = doc.stripped_origin(fid)

        if origin is None:
            return None

        inner_guid = doc.instance_source_guid(origin[0])
        return self.name(inner_guid, origin[1], depth + 1) if inner_guid else None


def same_number(left, right):
    """0 과 0.0 처럼 표기만 다른 값을 같은 것으로 봅니다."""
    try:
        return abs(float(left) - float(right)) < 1e-4
    except (TypeError, ValueError):
        return left == right


def collect_files(root):
    """폴더면 아래를 훑고, 파일 하나를 주면 그것만 봅니다."""
    if os.path.isfile(root):
        if root.endswith(SEARCH_EXT):
            yield root
        return

    for dirpath, _, filenames in os.walk(root):
        for filename in filenames:
            if filename.endswith(SEARCH_EXT):
                yield os.path.join(dirpath, filename)


def main():
    parser = argparse.ArgumentParser(description="프리팹 배치 오버라이드를 찾습니다.")
    parser.add_argument("--all", action="store_true", help="값이 같은(효과 없는) 오버라이드까지 봅니다.")
    parser.add_argument("--path", default="Assets", help="검사할 폴더나 파일 (기본 Assets)")
    args = parser.parse_args()

    guid_index = build_guid_index()
    resolver = Resolver(guid_index)

    total_differs = 0
    total_same = 0

    for path in sorted(collect_files(args.path)):
        doc = Document(path)

        if not doc.instances:
            continue

        differs = []
        same = 0

        for instance_fid, block in doc.instances:
            source_guid = SOURCE_PREFAB.search(block)

            if not source_guid:
                continue

            source_guid = source_guid.group(1)

            for target, guid, prop, value in MODIFICATION.findall(block):
                if prop not in LAYOUT_PROPERTIES:
                    continue

                original, from_layer = resolver.resolve(guid, target, prop)

                if original is None:
                    continue

                # 프리팹 루트의 원본 값은 자리표시자라 비교할 것이 못 됩니다.
                # 다만 중간 층이 자리를 정해 두었다면 그 값과 어긋나는지는 봐야 합니다 —
                # 프리팹을 고쳐도 화면이 안 바뀌는 일이 바로 여기서 납니다.
                if not from_layer and resolver.is_root(guid, target):
                    continue

                if same_number(original, value.strip()):
                    same += 1
                    if args.all:
                        differs.append((target, prop, original, value.strip(), True,
                                        resolver.name(guid, target)))
                else:
                    differs.append((target, prop, original, value.strip(), False,
                                    resolver.name(guid, target)))

        real = [row for row in differs if not row[4]]
        total_differs += len(real)
        total_same += same

        if not differs:
            continue

        print("\n%s" % path.replace("\\", "/"))
        print("   값이 다름 %d개 / 값이 같음 %d개" % (len(real), same))

        for target, prop, original, value, is_same, name in differs:
            mark = "같음" if is_same else "다름"
            print("   [%s] %-26s %-22s %s -> %s"
                  % (mark, (name or "&" + target)[:26], prop, original, value))

    print("\n%s" % ("=" * 60))
    print("값이 다른 배치 오버라이드 %d개 / 효과 없는 오버라이드 %d개" % (total_differs, total_same))

    return 0


if __name__ == "__main__":
    sys.exit(main())
