# ============================================================================
# MSP Unity Android Bridge — Build & copy AAR to Unity
# ============================================================================
# Usage:
#   make build      Build release AAR
#   make copy       Copy AAR to Unity Plugins/Android (requires build)
#   make install    Build + copy (default)
#   make clean      Remove bridge build outputs
#   make help       Show targets
#
# Override paths:
#   UNITY_PLUGINS_DIR=/path/to/Assets/Plugins/Android make install
#   GRADLEW=/path/to/gradlew make build
# ============================================================================

.PHONY: help build copy install clean check-gradlew check-unity-dir

SHELL := /bin/bash
ROOT_DIR := $(shell pwd)

AAR_NAME := msp-unity-bridge-release.aar
AAR_OUTPUT := $(ROOT_DIR)/bridge/build/outputs/aar/$(AAR_NAME)

# Default Unity demo project; override with UNITY_PLUGINS_DIR=...
UNITY_PLUGINS_DIR ?= $(abspath $(ROOT_DIR)/../demo/Assets/Plugins/Android)
UNITY_AAR_DEST := $(UNITY_PLUGINS_DIR)/$(AAR_NAME)

# Prefer ./gradlew; otherwise use `gradle` from PATH (override with GRADLEW=...)
GRADLEW ?= $(if $(wildcard $(ROOT_DIR)/gradlew),$(ROOT_DIR)/gradlew,gradle)

.DEFAULT_GOAL := install

help:
	@echo "MSP Unity Android Bridge"
	@echo ""
	@echo "  make install   Build release AAR and copy to Unity (default)"
	@echo "  make build     Build $(AAR_NAME)"
	@echo "  make copy      Copy built AAR to Unity Plugins/Android"
	@echo "  make clean     Remove bridge/build"
	@echo ""
	@echo "Paths:"
	@echo "  AAR output:  $(AAR_OUTPUT)"
	@echo "  Unity dest:  $(UNITY_AAR_DEST)"
	@echo "  Gradle:      $(GRADLEW)"
	@echo ""
	@echo "Override example:"
	@echo "  UNITY_PLUGINS_DIR=/path/to/Assets/Plugins/Android make install"

check-gradlew:
	@if [ -x "$(GRADLEW)" ]; then \
		: ; \
	elif command -v "$(GRADLEW)" >/dev/null 2>&1; then \
		: ; \
	else \
		echo "error: Gradle not found: $(GRADLEW)"; \
		echo "hint: install Gradle, run 'gradle wrapper' in android-bridge/, or set GRADLEW=/path/to/gradlew"; \
		exit 1; \
	fi

check-unity-dir:
	@mkdir -p "$(UNITY_PLUGINS_DIR)"
	@test -d "$(UNITY_PLUGINS_DIR)" || { \
		echo "error: Unity Plugins/Android directory not found: $(UNITY_PLUGINS_DIR)"; \
		echo "hint: set UNITY_PLUGINS_DIR to your Unity project's Assets/Plugins/Android"; \
		exit 1; \
	}

build: check-gradlew
	@echo "==> Building $(AAR_NAME)"
	"$(GRADLEW)" -p "$(ROOT_DIR)" :bridge:assembleRelease
	@test -f "$(AAR_OUTPUT)" || { \
		echo "error: expected AAR not found: $(AAR_OUTPUT)"; \
		exit 1; \
	}
	@echo "==> Built $(AAR_OUTPUT)"

copy: check-unity-dir
	@test -f "$(AAR_OUTPUT)" || { \
		echo "error: AAR not built yet. Run 'make build' first."; \
		exit 1; \
	}
	@echo "==> Copying to $(UNITY_AAR_DEST)"
	cp -f "$(AAR_OUTPUT)" "$(UNITY_AAR_DEST)"
	@echo "==> Done"

install: build copy

clean:
	@echo "==> Cleaning bridge/build"
	rm -rf "$(ROOT_DIR)/bridge/build"
	@echo "==> Done"
