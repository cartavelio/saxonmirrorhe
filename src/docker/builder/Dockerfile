FROM saxonica-centos8-core:latest
MAINTAINER Norman Walsh <norm@saxonica.com>

ADD jdk-8u192-linux-x64.rpm /tmp/
RUN yum localinstall -y /tmp/jdk-8u192-linux-x64.rpm
RUN rm -Rf /tmp/jdk-8u192-linux-x64.rpm

ADD jet15.3-linux-docker.tar.gz /usr/local

# Jet fails unless it can write to the directory where it was
# installed. (Who does that?) To fix this, we chown it to the
# docker-user user that is used for builds.
RUN chown -R docker-user /usr/local/jet15.3

RUN yum install -y ant

USER docker-user

ADD resources/jet.sh /tmp

CMD ["/bin/bash", "/tmp/jet.sh"]
