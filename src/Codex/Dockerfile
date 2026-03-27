FROM docker:27-cli

RUN apk add --no-cache nodejs npm bash git

WORKDIR /workspace/FamilyLedger

CMD ["sh", "-lc", "npm install -g @openai/codex && tail -f /dev/null"]
