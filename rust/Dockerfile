# syntax=docker/dockerfile:1

# ---- Build stage ---------------------------------------------------------
FROM rust:1-slim-bookworm AS build
WORKDIR /src
COPY Cargo.toml Cargo.lock ./
COPY src ./src
RUN cargo build --release --bin api

# ---- Runtime stage -------------------------------------------------------
FROM debian:bookworm-slim AS runtime
WORKDIR /app
RUN useradd -r -u 10001 appuser
COPY --from=build /src/target/release/api ./api
ENV PORT=8080
EXPOSE 8080
USER appuser
ENTRYPOINT ["/app/api"]
