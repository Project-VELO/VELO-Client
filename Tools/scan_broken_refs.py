# -*- coding: utf-8 -*-
"""저장소 안에서 존재하지 않는 에셋을 가리키는 참조를 찾는다.

    python Tools/scan_broken_refs.py            # 저장소 전체
    python Tools/scan_broken_refs.py --all      # 외부 에셋까지 포함

왜 필요한가
    프리팹이나 에셋의 이름을 바꿀 때 .meta 를 함께 옮기지 않으면 guid 가 새로 생긴다.
    그것을 물고 있던 쪽은 없는 guid 를 가리킨 채 남고, 유니티는 없는 참조를 조용히
    건너뛰므로 콘솔에 아무 경고도 남지 않는다. 화면에서 무언가 통째로 사라지는데
    오류는 없는 상태가 되어 원인을 찾기 어렵다.

    실제로 P_UI_Office 가 이름이 바뀐 P_UI_Panel_Today 를 옛 guid 로 물고 있어
    사무실의 오늘 스케줄 표가 통째로 보이지 않은 적이 있다.

읽는 법
    깨진 guid 와 그것을 참조하는 파일이 함께 나온다. 원래 어떤 파일이었는지는
    아래 명령으로 git 이력에서 찾을 수 있다.

        git log --all --diff-filter=A --name-only -S"<guid>" -- "*.meta"

    내부 fileID 가 그대로인 이름 변경이라면, 참조하는 쪽의 guid 만 새 것으로 바꾸면
    복구된다. 실제로 지워진 에셋이라면 참조를 지우거나 에셋을 되살려야 한다.
"""
import argparse
import collections
import os
import re
import sys

# 참조를 담고 있는 텍스트 에셋들. 바이너리 직렬화는 이 방식으로 읽을 수 없다.
SEARCH_EXT = (".prefab", ".unity", ".asset", ".mat", ".controller", ".anim",
              ".playable", ".overrideController", ".shadergraph", ".spriteatlas",
              ".spriteatlasv2", ".preset", ".inputactions", ".signal", ".mixer")

# guid 를 등록해 둔 곳. 패키지까지 봐야 내장 에셋 참조를 깨진 것으로 잘못 세지 않는다.
META_ROOTS = ("Assets", "Packages", os.path.join("Library", "PackageCache"))

SCAN_ROOTS = ("Assets", "ProjectSettings")

# 유니티 내장 리소스는 .meta 가 없고 guid 가 0으로 시작한다.
BUILTIN_PREFIX = "0000000000000000"

# 외부에서 받아 온 에셋은 데모 리소스가 빠진 채 들어오는 일이 흔해 기본적으로 뺀다.
EXTERNAL_MARK = os.path.join("20. External Assets", "")

GUID_META = re.compile(r"^guid: ([0-9a-f]{32})\s*$", re.M)
GUID_REF = re.compile(r"guid: ([0-9a-f]{32})")

# .meta 의 guid 는 파일 첫머리에 있다. 전체를 읽으면 폰트 에셋 등에서 크게 느려진다.
META_HEAD_BYTES = 400


def collect_known_guids():
    known = set()

    for root in META_ROOTS:
        if not os.path.isdir(root):
            continue

        for dirpath, _, filenames in os.walk(root):
            for filename in filenames:
                if not filename.endswith(".meta"):
                    continue

                try:
                    head = open(os.path.join(dirpath, filename),
                                encoding="utf-8", errors="ignore").read(META_HEAD_BYTES)
                except OSError:
                    continue

                found = GUID_META.search(head)

                if found:
                    known.add(found.group(1))

    return known


def collect_references(include_external):
    references = collections.defaultdict(set)

    for root in SCAN_ROOTS:
        if not os.path.isdir(root):
            continue

        for dirpath, _, filenames in os.walk(root):
            if not include_external and EXTERNAL_MARK in dirpath + os.sep:
                continue

            for filename in filenames:
                if root == "Assets" and not filename.endswith(SEARCH_EXT):
                    continue

                path = os.path.join(dirpath, filename)

                try:
                    body = open(path, encoding="utf-8", errors="ignore").read()
                except OSError:
                    continue

                for guid in set(GUID_REF.findall(body)):
                    references[guid].add(path)

    return references


def main():
    parser = argparse.ArgumentParser(description="깨진 에셋 참조를 찾습니다.")
    parser.add_argument("--all", action="store_true",
                        help="20. External Assets 까지 함께 검사합니다.")
    args = parser.parse_args()

    known = collect_known_guids()
    references = collect_references(args.all)

    print("알려진 guid %d개 / 참조된 guid %d개" % (len(known), len(references)))

    broken = {guid: sorted(paths) for guid, paths in references.items()
              if guid not in known and not guid.startswith(BUILTIN_PREFIX)}

    if not broken:
        print("깨진 참조 없음")
        return 0

    print("\n깨진 참조 %d개" % len(broken))

    # 많이 참조된 것부터 본다. 널리 쓰이던 에셋일수록 피해가 크다.
    for guid, paths in sorted(broken.items(), key=lambda item: -len(item[1])):
        print("\n%s  (참조 파일 %d개)" % (guid, len(paths)))

        for path in paths[:10]:
            print("   " + path.replace("\\", "/"))

        if len(paths) > 10:
            print("   ... 외 %d개" % (len(paths) - 10))

    return 1


if __name__ == "__main__":
    sys.exit(main())
