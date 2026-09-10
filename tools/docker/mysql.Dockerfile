FROM mysql:latest as mysql-tests

COPY --from=bootstrap ./mysql/setup.sql /docker-entrypoint-initdb.d/
