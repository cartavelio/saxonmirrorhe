FROM quay.io/pypa/manylinux_2_28_aarch64 AS saxon-prereqs
LABEL authors="matt"
LABEL com.saxonica.project=saxonc

ARG UID
ARG GID

# We're building off the manylinux base image, which is AlmaLinux (RHEL derived)

RUN dnf -y install --allowerasing \
    sudo \
    java-11-openjdk-headless \
    valgrind \
    bzip2-devel \
    libffi-devel \
    ncurses-devel \
    openssl-devel \
    readline-devel \
    sqlite-devel \
    xz-devel \
    && \
    dnf clean all && \
    rm -rf /var/cache/yum

# Configure environment
RUN echo "GID: ${GID}" && if getent group ${GID} > /dev/null; [ $? -ne 0 ]; then groupadd --gid=${GID} saxonica; fi && \
    useradd --uid=${UID} --gid=${GID} --create-home saxonica --home-dir /opt/saxonica && \
    echo 'saxonica ALL=NOPASSWD: ALL' >> /etc/sudoers.d/50-saxonica && \
    sudo -u saxonica mkdir /opt/saxonica/bin

ENV PATH=/opt/saxonica/bin:$PATH


FROM saxon-prereqs AS saxon-graalvm

RUN mkdir -p /opt && \
    curl -sSL https://github.com/graalvm/graalvm-ce-builds/releases/download/vm-22.3.1/graalvm-ce-java11-linux-aarch64-22.3.1.tar.gz | tar -xzf - -C /opt && \
    /opt/graalvm-ce-java11-22.3.1/bin/gu install native-image

FROM saxon-graalvm AS saxon-gradle

# Run commands as saxonica user
USER saxonica

# Bootstrap Gradle as the saxonica user
RUN mkdir -p /opt/saxonica/gradle
# bootstrap Gradle X
ENV GRADLE_USER_HOME=/opt/saxonica/gradle

VOLUME /opt/saxonica/gradle

FROM saxon-gradle AS saxon-python

# Run commands as saxonica user
USER saxonica

# Stop Git complaining about safety
RUN git config --global --add safe.directory /opt/saxonica/saxondev

# Setup valgrind-enabled python debug build recipe for pyenv
RUN mkdir -p /opt/saxonica/python-build

ADD 3.12.2-valgrind /opt/saxonica/python-build/

WORKDIR /opt/saxonica

# Install and set up pyenv
ENV PYENV_ROOT=/opt/saxonica/pyenv \
    PATH=/home/saxonica/bin:/opt/saxonica/pyenv/bin:$PATH

RUN curl -sSL "https://github.com/pyenv/pyenv-installer/raw/master/bin/pyenv-installer" | bash && \
    echo 'export PYENV_ROOT="/opt/saxonica/pyenv"' >> ~/.bashrc && \
    echo '[[ -d $PYENV_ROOT/bin ]] && export PATH="$PYENV_ROOT/bin:$PATH"' >> ~/.bashrc && \
    echo 'eval "$(pyenv init -)"' >> ~/.bashrc && \
    echo 'export PYENV_ROOT="/opt/saxonica/pyenv"' >> ~/.profile  && \
    echo '[[ -d $PYENV_ROOT/bin ]] && export PATH="$PYENV_ROOT/bin:$PATH"' >> ~/.profile && \
    echo 'eval "$(pyenv init -)"' >> ~/.profile

RUN pyenv install /opt/saxonica/python-build/3.12.2-valgrind
RUN sudo rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*


#RUN pyenv install /opt/saxonica/python-build/3.12.2-valgrind && \
#    sudo rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*
VOLUME /opt/saxonica/pyenv/versions
VOLUME /opt/saxonica/pyenv/shims

FROM saxon-python AS saxon-dev

# Run commands as saxonica user
USER saxonica

# Setup valgrind-enabled python debug build recipe for pyenv
RUN mkdir -p /opt/saxonica/saxondev/buildSrc/.gradle /opt/saxonica/saxondev/buildSrc/build /opt/saxonica/saxondev/build /opt/saxonica/saxondev/.gradle

VOLUME /opt/saxonica/saxondev
VOLUME /opt/saxonica/saxondev/build
VOLUME /opt/saxonica/saxondev/.gradle
VOLUME /opt/saxonica/saxondev/buildSrc/build
VOLUME /opt/saxonica/saxondev/buildSrc/.gradle

WORKDIR /opt/saxonica/saxondev
