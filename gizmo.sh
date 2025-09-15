#!/bin/sh

saxondev_dir="$(dirname $0)"

OPTSTRING=":l:e:s:q:h"

license_dir=""
edition="ee"

script_arg=""
query_arg=""

while getopts $OPTSTRING opt; do
  case $opt in
    l)
      license_dir="$OPTARG"
      ;;
    e)
      edition="$OPTARG"
      ;;
    h)
      echo "USAGE: $0 [-l SAXON_LICENSE_DIR] [-e EDITION] [GIZMO_ARG ...]" 1>&2
      exit 1
      ;;
    s)
      script_arg="-s$OPTARG"
      ;;
    q)
      query_arg="-q$OPTARG"
      ;;
  esac
done
echo $OPTIND

cd $saxondev_dir

# If custom license dir specified
if [ ! -z "$license_dir" ]; then
  license_args="-PsaxonLicenseDir=${license_dir}"
  echo "- Using license dir ${license_dir}" 1>&2
else
  license_args=""
fi

edition_task=""
if [ "$edition" = "ee" ]; then
  echo "- Using ee" 1>&2
else
  echo "- Using $edition" 1>&2
  edition_task=":$edition"
fi

task_args=""
if [ ! -z "$script_arg" ]; then
  task_args="$script_arg "
fi

if [ ! -z "$query_arg" ]; then
  task_args="${task_args}${query_arg}"
fi

if [ -z "$task_args" ]; then
  ./gradlew --console=plain -w "java${edition_task}:gizmo" "$license_args"
else
  echo "gizmo $task_args" 1>&2
  ./gradlew --console=plain -w "java${edition_task}:gizmo" "$license_args" --args="$task_args"
fi
