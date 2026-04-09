#!/usr/bin/env bash
# =============================================================================
# Brew — Git Pre-Commit Hook
# =============================================================================
#
# Installation:
#   cp hooks/pre-commit.sh .git/hooks/pre-commit
#   chmod +x .git/hooks/pre-commit
#
# Or create a symlink:
#   ln -sf ../../hooks/pre-commit.sh .git/hooks/pre-commit
#
# What it checks:
#   1. Level JSON files — validates schema structure
#   2. Economy file changes — reminds to update economy-model.md
#   3. Hardcoded currency values in C# — warns about magic numbers
#   4. Large files — blocks files > 10MB
#   5. Credentials / secrets — blocks .env and credential files
# =============================================================================

set -euo pipefail

RED='\033[0;31m'
YELLOW='\033[0;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

PASS=0
FAIL=0
WARN=0

pass() { echo -e "  ${GREEN}PASS${NC}  $1"; PASS=$((PASS + 1)); }
fail() { echo -e "  ${RED}FAIL${NC}  $1"; FAIL=$((FAIL + 1)); }
warn() { echo -e "  ${YELLOW}WARN${NC}  $1"; WARN=$((WARN + 1)); }

echo "════════════════════════════════════════════"
echo "  Brew Pre-Commit Checks"
echo "════════════════════════════════════════════"

STAGED_FILES=$(git diff --cached --name-only --diff-filter=ACM)

# ─── 1. Level JSON Validation ─────────────────────────────────────────────────

LEVEL_JSONS=$(echo "$STAGED_FILES" | grep -E '\.json$' | grep -iE 'level' || true)

if [ -n "$LEVEL_JSONS" ]; then
    echo ""
    echo "  Checking level JSON files..."
    for f in $LEVEL_JSONS; do
        if [ ! -f "$f" ]; then
            continue
        fi

        # Check valid JSON
        if ! python3 -c "import json; json.load(open('$f'))" 2>/dev/null; then
            fail "$f — invalid JSON syntax"
            continue
        fi

        # Check required keys exist
        MISSING=$(python3 -c "
import json, sys
required = ['level_id', 'grid_width', 'grid_height', 'ingredient_pool',
            'recipe_targets', 'move_limit', 'star_thresholds']
with open('$f') as fh:
    data = json.load(fh)
missing = [k for k in required if k not in data]
if missing:
    print(', '.join(missing))
" 2>/dev/null || echo "PARSE_ERROR")

        if [ "$MISSING" = "PARSE_ERROR" ]; then
            fail "$f — could not parse for schema check"
        elif [ -n "$MISSING" ]; then
            fail "$f — missing keys: $MISSING"
        else
            pass "$f — schema OK"
        fi
    done
else
    pass "No level JSON files staged"
fi

# ─── 2. Economy File Change Reminder ─────────────────────────────────────────

ECONOMY_FILES=$(echo "$STAGED_FILES" | grep -iE '(economy|currency|workshop|booster|reward)' || true)
ECONOMY_DOC_CHANGED=$(echo "$STAGED_FILES" | grep -F 'economy-model.md' || true)

if [ -n "$ECONOMY_FILES" ] && [ -z "$ECONOMY_DOC_CHANGED" ]; then
    echo ""
    warn "Economy-related files changed but docs/economy/economy-model.md was not updated."
    echo "         Changed files:"
    echo "$ECONOMY_FILES" | while read -r ef; do
        echo "           - $ef"
    done
    echo "         Please verify the doc is still accurate."
fi

# ─── 3. Hardcoded Currency Values in C# ──────────────────────────────────────

CS_FILES=$(echo "$STAGED_FILES" | grep -E '\.cs$' || true)

if [ -n "$CS_FILES" ]; then
    echo ""
    echo "  Checking C# files for hardcoded currency values..."
    FOUND_HARDCODED=false

    for f in $CS_FILES; do
        if [ ! -f "$f" ]; then
            continue
        fi

        HITS=$(grep -nE -i '(essence|gem|coin|currency)\s*[=+\-]\s*[0-9]{2,}' "$f" || true)
        if [ -n "$HITS" ]; then
            warn "$f — possible hardcoded currency values:"
            echo "$HITS" | while read -r line; do
                echo "           $line"
            done
            FOUND_HARDCODED=true
        fi
    done

    if [ "$FOUND_HARDCODED" = false ]; then
        pass "No hardcoded currency patterns in C# files"
    fi
else
    pass "No C# files staged"
fi

# ─── 4. Large File Check (>10MB) ─────────────────────────────────────────────

echo ""
echo "  Checking for large files..."
LARGE_FOUND=false

for f in $STAGED_FILES; do
    if [ ! -f "$f" ]; then
        continue
    fi

    SIZE=$(wc -c < "$f" 2>/dev/null || echo 0)
    MAX_SIZE=$((10 * 1024 * 1024))  # 10 MB

    if [ "$SIZE" -gt "$MAX_SIZE" ]; then
        SIZE_MB=$(echo "scale=1; $SIZE / 1048576" | bc 2>/dev/null || echo "${SIZE}B")
        fail "$f — ${SIZE_MB}MB exceeds 10MB limit. Use Git LFS for large assets."
        LARGE_FOUND=true
    fi
done

if [ "$LARGE_FOUND" = false ]; then
    pass "No files exceed 10MB"
fi

# ─── 5. Credentials / Secrets Check ──────────────────────────────────────────

echo ""
echo "  Checking for credential files..."
CRED_FILES=$(echo "$STAGED_FILES" | grep -E '(\.env$|\.env\.|credentials\.json|service.account.*\.json|.*\.keystore|.*\.jks|.*\.pem$|.*_rsa$)' || true)

if [ -n "$CRED_FILES" ]; then
    for cf in $CRED_FILES; do
        fail "$cf — credentials/secrets must not be committed"
    done
else
    pass "No credential files staged"
fi

# ─── Summary ─────────────────────────────────────────────────────────────────

echo ""
echo "════════════════════════════════════════════"
echo -e "  Results: ${GREEN}${PASS} passed${NC}, ${RED}${FAIL} failed${NC}, ${YELLOW}${WARN} warnings${NC}"
echo "════════════════════════════════════════════"

if [ "$FAIL" -gt 0 ]; then
    echo -e "  ${RED}Commit blocked.${NC} Fix the failures above and try again."
    echo "  To bypass (not recommended): git commit --no-verify"
    exit 1
fi

if [ "$WARN" -gt 0 ]; then
    echo -e "  ${YELLOW}Warnings present${NC} — commit allowed but please review."
fi

exit 0
