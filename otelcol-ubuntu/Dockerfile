FROM ubuntu:22.04

#WORKDIR /root
RUN pwd
RUN apt-get update
RUN apt-get -y install wget systemctl curl
RUN wget https://github.com/open-telemetry/opentelemetry-collector-releases/releases/download/v0.89.0/otelcol-contrib_0.89.0_linux_amd64.deb
RUN ls
RUN dpkg -i otelcol-contrib_0.89.0_linux_amd64.deb
#RUN wget https://github.com/open-telemetry/opentelemetry-collector-contrib-releases/releases/download/v0.89.0/otelcol_0.89.0_linux_amd64.deb
#RUN dpkg -i otelcol_0.89.0_linux_amd64.deb

#ENTRYPOINT [ "sh" ]
