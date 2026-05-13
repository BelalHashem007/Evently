---
alwaysApply: true
---

# Project Planning & Persistence Rules

## Purpose

This project uses persistent development plans to maintain continuity across Cursor sessions and prevent losing progress when chat context resets.

The `plans/` folder is the single source of truth for project progress.

---

# Core Rules

## 1. Always Save Plans

Whenever a roadmap, implementation plan, feature breakdown, architecture decision, or execution checklist is created:

- Save it inside:

```txt
plans/
```

- Use markdown format (`.md`)

---

## 2. Naming Convention

Use this filename format:

```txt
plans/YYYY-MM-DD-short-plan-name.md
```

Example:

```txt
plans/2026-05-13-event-booking-system.md
```

---

## 3. Required Plan Structure

Every plan file must contain the following sections:

```md
# Plan Title

## Goal

## Current Status

## Completed Tasks

## Remaining Tasks

## Decisions Made

## Risks / Notes

## Next Immediate Action
```

---

## 4. Updating Existing Plans

When work continues on an existing feature or roadmap:

- Update the existing plan file
- Do NOT create duplicate plans
- Mark completed tasks clearly
- Add new decisions and context
- Keep "Next Immediate Action" updated

---

## 5. Session Continuity

At the start of every new session:

1. Check the `plans/` folder
2. Find unfinished plans
3. Load the most recent active plan
4. Continue from:
   - `Current Status`
   - `Remaining Tasks`
   - `Next Immediate Action`

If multiple unfinished plans exist:

- Ask which plan should continue

---

## 6. Source of Truth Rule

Conversation memory is temporary.

The markdown files inside `plans/` are the authoritative source of:

- progress
- architecture decisions
- pending work
- implementation order

Never rely only on chat context.

---

## 7. Task Tracking Rules

Use markdown checkboxes:

```md
- [x] Completed task
- [ ] Pending task
```

Keep tasks granular and implementation-focused.

---

## 8. Decision Logging

Important technical decisions must be documented under:

```md
## Decisions Made
```

Include:

- chosen architecture
- libraries/frameworks
- database strategy
- authentication approach
- API patterns
- major tradeoffs

---

## 9. End-of-Session Requirement

Before ending a work session:

- Update the current plan
- Mark completed items
- Add new findings
- Write the next actionable step

The next session should be able to resume immediately without rereading the full chat history.

---

# Example

```md
# Event Ticket Booking System

## Goal

Build a scalable event booking platform using ASP.NET MVC.

## Current Status

Authentication completed.

## Completed Tasks

- [x] ASP.NET MVC setup
- [x] SQL Server connection
- [x] EF Core migrations
- [x] ASP.NET Identity integration

## Remaining Tasks

- [ ] Event CRUD
- [ ] Ticket purchasing flow
- [ ] Admin dashboard

## Decisions Made

- Using ASP.NET Identity
- Using EF Core Code First
- Using Repository Pattern

## Risks / Notes

- Payment integration not selected yet

## Next Immediate Action

Implement Event CRUD endpoints and admin UI.
```
