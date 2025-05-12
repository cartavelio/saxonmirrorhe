# 6 November 2020, centos8=8.2.2004
FROM centos:centos8
MAINTAINER Norman Walsh <norm@saxonica.com>

RUN dnf -y install epel-release
RUN dnf -y clean all
RUN dnf -y update

## java
#ADD resources/jdk-8u192-linux-x64.rpm /tmp/
#RUN yum localinstall -y /tmp/jdk-8u192-linux-x64.rpm
#RUN rm -Rf /tmp/jdk-8u192-linux-x64.rpm

RUN dnf -y groupinstall 'Development tools'

RUN dnf -y install openssh-server glibc.i686 gdb.x86_64 redhat-lsb.x86_64 \
                   zlib-devel bzip2-devel openssl-devel ncurses-devel \
                   sqlite-devel readline-devel tk-devel gdbm-devel \
                   xz-devel libstdc++-devel glibc-devel sudo libicu \
                   libstdc++-devel.i686 glibc-devel.i686 \
                   flex bison cyrus-sasl-devel emacs \
                   gcc gcc-c++ libxml2 ack git git-svn wget uuid libffi-devel

RUN yum install -y python38

# generate ssh key
RUN ssh-keygen -q -N "" -t dsa -f /etc/ssh/ssh_host_dsa_key
RUN ssh-keygen -q -N "" -t rsa -f /etc/ssh/ssh_host_rsa_key
RUN sed -ri 's/session    required     pam_loginuid.so/#session    required     pam_loginuid.so/g' /etc/pam.d/sshd
RUN mkdir -p /root/.ssh && chown root.root /root && chmod 700 /root/.ssh

# other
RUN adduser --gid users --uid 1000 docker-user
RUN usermod -a -G wheel docker-user
ADD resources/sudoers /etc/sudoers
RUN chown root.root /etc/sudoers

WORKDIR /home/docker-user

ENV TERM screen-256color
#ENV JAVA_HOME /usr/java/latest
