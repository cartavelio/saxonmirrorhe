FROM ubuntu:latest AS saxon-prereqs
LABEL authors="matt"
LABEL com.saxonica.project=saxonc

ARG UID
ARG GID
# Crib much of this from CircleCI's base and python docker files
# Change default shell for RUN from Dash to Bash
SHELL ["/bin/bash", "-exo", "pipefail", "-c"]

ENV DEBIAN_FRONTEND=noninteractive \
    TERM=dumb \
    PAGER=cat

# Configure environment
RUN echo 'APT::Get::Assume-Yes "true";' > /etc/apt/apt.conf.d/90saxonica && \
	echo 'DPkg::Options "--force-confnew";' >> /etc/apt/apt.conf.d/90saxonica && \
	apt-get update && apt-get install -y \
        curl \
        locales \
        sudo \
	&& \
    mkdir -p /opt && \
	locale-gen en_US.UTF-8 && \
	rm -rf /var/lib/apt/lists/* && \
    if [ ! getent group ${GID} ]; then groupadd --gid=${GID} saxonica; fi && \
    useradd --uid=${UID} --gid=${GID} --create-home saxonica --home-dir /opt/saxonica && \
    echo 'saxonica ALL=NOPASSWD: ALL' >> /etc/sudoers.d/50-saxonica && \
    echo 'Defaults    env_keep += "DEBIAN_FRONTEND"' >> /etc/sudoers.d/env_keep && \
    sudo -u saxonica mkdir /opt/saxonica/bin

ENV PATH=/opt/saxonica/bin:$PATH \
	LANG=en_US.UTF-8 \
	LANGUAGE=en_US:en \
	LC_ALL=en_US.UTF-8

RUN apt-get update && apt-get install -y --no-install-recommends \
		build-essential \
		ca-certificates \
		cmake \
		file \
		gettext-base \
		gzip \
		jq \
		libcurl4-openssl-dev \
		libssl-dev \
		libsqlite3-dev \
        libbz2-dev \
        liblzma-dev \
        libncurses5-dev \
        libncursesw5-dev \
        libreadline-dev \
        libffi-dev \
        libsqlite3-dev \
        libssl-dev \
        libxml2-dev \
        libxmlsec1-dev \
        llvm \
		lsof \
        make \
        openjdk-11-jdk-headless \
		pkg-config \
        python3-openssl \
		retry \
		shellcheck \
		software-properties-common \
        tk-dev \
        unzip \
        valgrind \
		vim \
		wget \
        wget \
        xz-utils \
		zip \
        zlib1g-dev && \
	# get the semi-official latest-stable git instead of using the old(er) version from the ubuntu distro
	add-apt-repository ppa:git-core/ppa && apt-get install -y git && \
    git config --global --add safe.directory /opt/saxonica/saxondev && \
	rm -rf /var/lib/apt/lists/*

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

# Setup valgrind-enabled python debug build recipe for pyenv
RUN mkdir -p /opt/saxonica/python-build

ADD 3.12.2-valgrind /opt/saxonica/python-build/

WORKDIR /home/saxonica

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

RUN pyenv install /opt/saxonica/python-build/3.12.2-valgrind && \
    sudo rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

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

ENTRYPOINT ["/bin/bash"]