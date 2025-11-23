# README.md  
**SaaS Template — Project Structure Overview**

This repository provides a clean, modular SaaS starter template combining:

- **React SPA** for the application dashboard  
- **ASP.NET Web API** as the backend  
- **Static landing site**  
- **Bootstrap scripts** for generating new projects  
- **Documentation** for developers and AI agents  

---

## 📁 Folder Structure

```
/bootstrap       → Automation scripts for creating new SaaS projects
/src             → Source code for application (frontend + backend + landing)
/docs            → Documentation, guides, AGENTS.md, diagrams
```

---

## 📁 `/bootstrap` — Project Automation

Contains scripts and templates used to:

- initialize a new SaaS project,
- generate configuration files,
- prepare environment variables,
- scaffold backend/frontend structure,
- bootstrap CI/CD pipelines.

This folder allows you to clone the repo once and quickly spin up new SaaS products.

---

## 📁 `/src` — Main Application Code

```
/src
  ├── app/         → React SPA + ASP.NET Web API backend
  ├── wwwroot/     → Static landing website (public pages)
  └── ...
```

### 🔸 `/src/app` — Application (Frontend + Backend)

Contains the core application functionality:

- **React SPA** (Vite + TypeScript) — dashboard, user account, admin UI  
- **ASP.NET Web API** — backend, authentication, business logic, DB access  

These two layers communicate via REST endpoints.

Typical structure inside `/src/app`:

```
/src/app/frontend/     → React SPA
/src/app/backend/      → ASP.NET Web API
```

---

## 📁 `/src/wwwroot` — Static Landing Site

This folder contains the **public marketing site**:

- pure HTML/CSS/JS, or  
- Hugo/Eleventy/other static generator output  

It is completely independent of the main React application.  
Ideal for hosting:

- landing page,
- pricing page,
- blog or documentation,
- promotional content.

---

## 📁 `/docs` — Documentation

This folder includes:

- architecture and design docs,
- AGENTS.md (AI usage instructions),
- diagrams,
- onboarding guides,
- product specifications.

All non-executable knowledge lives here.

---

## ✔️ Summary

This template provides a clean and scalable structure:

- `/bootstrap` → tooling  
- `/src/app` → product backend + front-end  
- `/src/wwwroot` → public landing  
- `/docs` → documentation  

Designed for rapid SaaS development with full support for AI-assisted coding.
