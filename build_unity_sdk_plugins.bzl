"""This module builds ARCore SDK for Unity dependencies and copies them into the SDK"""

def get_target_path(plugin_dependency_info):
    if "output_regex" in plugin_dependency_info:
        return "TARGET_PATH=`grep -o -e \"[^ ]*{regex}[^ ]*\" -e \"[^ ]*\\.aar\" <<<\"$(locations {target})\"`;"
    else:
        return "TARGET_PATH=$(locations {target});"

def get_copy_command(plugin_dependency_info):
    if plugin_dependency_info.get("unzip", False):
        return "$(location //third_party/unzip:unzip) $$TARGET_PATH -d $$temp_dir/{path};"
    else:
        return "cp $$TARGET_PATH $$temp_dir/{path};"

def get_full_copy_command(plugin_dependency_infos):
    full_copy_command = "".join(
        [
            ("mkdir -p $$temp_dir/{path};" +
             get_target_path(info) +
             get_copy_command(info))
                .format(
                path = info["sdk_path"],
                target = info["target"],
                regex = info.get("output_regex", ""),
            )
            for info in plugin_dependency_infos
        ],
    )

    return full_copy_command

def build_unity_sdk_plugins(name, dependency_infos, out, visibility):
    plugin_dependency_build_targets = [dependency["target"] for dependency in dependency_infos]
    full_copy_command = get_full_copy_command(dependency_infos)

    native.genrule(
        name = name,
        srcs = plugin_dependency_build_targets,
        outs = [out],
        cmd = ("temp_dir=\"$$(mktemp -d)\";" +
               full_copy_command +
               "pushd $$temp_dir; tar cvfh {out_file} *; popd;" +
               "cp $$temp_dir/{out_file} $@").format(out_file = out),
        visibility = visibility,
        tools = ["//third_party/unzip:unzip"],
    )
