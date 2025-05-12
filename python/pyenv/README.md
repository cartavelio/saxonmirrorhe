# Running Saxon C testing with docker

This was mostly intended for running valgrind against SaxonC, which requires Linux.

On a Mac:

```shell
$ GID="$(id -g)" UID="$(id -u)" docker compose build saxondev
$ docker compose run saxondev
[saxonica@9e32e1347eef saxondev]$ ./gradlew eec_python_install_debug_wheel 
[saxonica@9e32e1347eef saxondev]$ ./build/pyvenv/saxon12-debug/bin/pip install psutil 
[saxonica@9e32e1347eef saxondev]$ ./build/pyvenv/saxon12-debug/bin/pip install psutil 
```

```shell
valgrind --tool=memcheck --track-origins=yes --show-leak-kinds=all --trace-children=yes --leak-check=full --log-file=...  build/pyvenv/.../bin/python3 ... 
```

