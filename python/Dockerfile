# syntax=docker/dockerfile:1

FROM python:3.12-slim AS runtime
WORKDIR /app
ENV PYTHONUNBUFFERED=1 PORT=8080

COPY pyproject.toml README.md ./
COPY src ./src
RUN pip install --no-cache-dir .

RUN useradd -r -u 10001 appuser
USER appuser
EXPOSE 8080
CMD ["python", "-m", "fhir_openehr_bridge.api"]
