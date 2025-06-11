import configparser
import tomllib
import pathlib
import sys

pytest_ini_path = pathlib.Path(sys.argv[1])

pyproject_toml = pathlib.Path(__file__).parent.joinpath("pyproject.toml")
with (pyproject_toml.open("rb") as f):
    data = tomllib.load(f)
    print(data["tool"]["pytest"]["ini_options"])
    pytest_ini_entries = data["tool"]["pytest"]["ini_options"]

    config = configparser.ConfigParser()
    pytest = {}
    for k, v in pytest_ini_entries.items():
        if isinstance(v, list):
          pytest[k] = "\n".join(v)
    config['pytest'] = pytest

    with pytest_ini_path.open('w') as f:
        config.write(f)
