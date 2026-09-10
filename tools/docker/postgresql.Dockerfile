FROM postgres:latest as postgres-tests

COPY --from=bootstrap ./postgresql/setup.sql /docker-entrypoint-initdb.d/
