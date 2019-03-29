"""This module builds ARCore SDK for Unity dependencies and copies them into the SDK"""

def get_target_path(info):
    if "output_regex" in info:
        return "TARGET_PATH=`grep -o -e \"[^ ]*{regex}[^ ]*\" -e \"[^ ]*\\.aar\" <<<\"$(locations {target})\"`;"
    else:
        return "TARGET_PATH=$(locations {target});"

def generate_copy_command(plugin_dependency_infos):
    copy_command = "".join(
        [
            ("mkdir -p $$temp_dir/{path};" +
             get_target_path(info) +
             "cp $$TARGET_PATH $$temp_dir/{path};")
                .format(
                path = info["sdk_path"],
                target = info["target"],
                regex = info.get("output_regex", ""),
            )
            for info in plugin_dependency_infos
        ],
    )

    return copy_command

def build_unity_sdk_plugins(name, dependency_infos, out, visibility):
    plugin_dependency_build_targets = [dependency["target"] for dependency in dependency_infos]
    copy_command = generate_copy_command(dependency_infos)

    native.genrule(
        name = name,
        srcs = plugin_dependency_build_targets,
        outs = [out],
        cmd = ("temp_dir=\"$$(mktemp -d)\";" +
               copy_command +
               "pushd $$temp_dir; tar cvfh {out_file} *; popd;" +
               "cp $$temp_dir/{out_file} $@").format(out_file = out),
        visibility = visibility,
    )
