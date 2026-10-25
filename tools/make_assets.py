#!/usr/bin/env python3
"""
生成 obj/project.assets.json（无 NuGet 包版）。

为什么需要它：
    当前宿主环境（WorkBuddy 启动的 shell）缺 LOCALAPPDATA 等环境变量，
    NuGet 的 NuGetEnvironment.CalculateFolderPath 会 Path.Combine(null, ...)
    抛 "Value cannot be null (Parameter 'path1')"，
    导致**任何** dotnet build 的隐式 restore 都崩（SDK 6/8/10 全试过）。

    本项目的两个工程都**不引用任何 NuGet 包**，所以只要有一份合法的
    assets.json，用 `dotnet build --no-restore` 就能正常构建。
    这份文件结构照抄 NuGet 自己的格式，用 net8.0 那份当模板。

用法： python3 tools/make_assets.py <项目目录> <项目名> <目标框架>
"""
import json, io, os, sys

def main():
    proj_dir, proj_name, tfm = sys.argv[1], sys.argv[2], sys.argv[3]
    root = os.path.abspath(proj_dir)
    obj = os.path.join(root, 'obj')
    os.makedirs(obj, exist_ok=True)

    # 模板：优先用 Tests 的（net8.0，今天生成过、格式正确）；没有就自己造
    tpl_path = os.path.join(os.path.dirname(root), 'FHSolver.Tests', 'obj', 'project.assets.json')
    tpl = None
    if os.path.exists(tpl_path):
        try:
            tpl = json.load(io.open(tpl_path, encoding='utf-8-sig'))
        except Exception:
            tpl = None
    if tpl is None:
        tpl = {"packageFolders": {"C://Users//hanf2//.nuget//packages//": {}},
               "project": {"restore": {
                   "fallbackFolders": ["D://VisualStudioSetup//Shared//NuGetPackages"],
                   "configFilePaths": ["C://Users//hanf2//AppData//Roaming//NuGet//NuGet.Config"],
                   "sources": {"https://api.nuget.org/v3/index.json": {}},
                   "warningProperties": {"warnAsError": ["NU1605"]}}}}

    r = tpl["project"]["restore"]
    win = root.replace('/', '\\')
    out = {
        "version": 4,
        "targets": {tfm: {}},
        "libraries": {},
        "projectFileDependencyGroups": {tfm: []},
        "packageFolders": tpl["packageFolders"],
        "project": {
            "version": "1.0.0",
            "restore": {
                "projectUniqueName": win + "\\" + proj_name + ".csproj",
                "projectName": proj_name,
                "projectPath": win + "\\" + proj_name + ".csproj",
                "packagesPath": "C://Users//hanf2//.nuget//packages//",
                "outputPath": win + "\\obj\\",
                "projectStyle": "PackageReference",
                "fallbackFolders": r.get("fallbackFolders", []),
                "configFilePaths": r.get("configFilePaths", []),
                "originalTargetFrameworks": [tfm],
                "sources": r.get("sources", {}),
                "frameworks": {tfm: {"framework": tfm, "targetAlias": tfm, "projectReferences": {}}},
                "warningProperties": r.get("warningProperties", {"warnAsError": ["NU1605"]}),
                "restoreAuditProperties": r.get("restoreAuditProperties", {}),
                "SdkAnalysisLevel": r.get("SdkAnalysisLevel", "10.0.400"),
            },
            "frameworks": {tfm: {
                "framework": tfm,
                "targetAlias": tfm,
                "imports": ["net461", "net462", "net47", "net471", "net48", "net481"] if tfm == "net472" else [],
                "warn": True,
                "runtimeIdentifierGraphPath": "C://Program Files//dotnet//sdk//10.0.401/PortableRuntimeIdentifierGraph.json",
            }},
        },
    }
    path = os.path.join(obj, 'project.assets.json')
    io.open(path, 'w', encoding='utf-8').write(json.dumps(out, indent=2, ensure_ascii=False))
    print('assets.json ->', path)

if __name__ == '__main__':
    main()
