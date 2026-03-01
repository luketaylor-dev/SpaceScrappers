# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SpaceScrappers is a **Unity 6000.3.2f1** multiplayer co-op game using **Unity Netcode for GameObjects (NGO) 2.7.0** and the **Universal Render Pipeline (URP 17.3.0)**. All gameplay scripts use the `SpaceScrappers.*` namespace.

## Key Packages

- **Unity Netcode for GameObjects** 2.7.0 — multiplayer networking
- **Unity Input System** 1.17.0 — input handling via `InputActionReference`
- **URP** 17.3.0 — rendering pipeline (relevant for shaders)
- **ParrelSync** — multi-editor co-op testing (see README.md for setup)
- **Odin Inspector** (Sirenix) — serialization/inspector tooling

## Testing Multiplayer

Use **ParrelSync** (`ParrelSync > Clones Manager`) for editor-based co-op testing — it's the fastest iteration loop. Builds can be created via `Tools > Quick Build Co-op`.

## Architecture

### Networking Pattern


## Conventions

## Game Design Context

## Common Tasks

## What NOT to do