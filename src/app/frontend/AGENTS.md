# AGENTS.md  
**Front-End Architecture & AI Agent Guidelines**  
**Stack: React + Vite + TypeScript + REST ASP.NET Web API**

---

## 1. Overview

This document defines the **core concepts**, **file structure**, **technology stack**, and **AI agent usage patterns** for the front-end client of the application.  
The goal is to provide a **consistent, predictable architecture** that AI agents (GitHub Copilot, ChatGPT, etc.) can reliably extend, refactor, and generate code for.

The front-end is implemented as a **pure SPA (Single Page Application)** using:

- **React 18+**
- **TypeScript**
- **Vite** (build + dev server)
- **React Router v6**
- **TanStack Query (React Query)** for API data fetching & caching
- **React Hook Form + Zod** for forms & validation
- **Tailwind CSS + shadcn/ui** for UI components
- **Zustand (optional)** for global client-side state
- **REST API (ASP.NET Web API)** as the backend

No SSR (Server-Side Rendering).  
Authentication is cookie-based (HttpOnly), with ASP.NET handling identity and issuing secure cookies.

---

## 2. Technology Overview

### React (SPA Model)
- All rendering happens on the client.
- No Next.js server components, no SSR complexity.
- React Router handles navigation inside the dashboard.

### Vite
- Lightning-fast dev server.
- Zero-config TypeScript support.
- Outputs static files suitable for Nginx or any static host.

### REST API Integration
- All communication with the backend occurs through:
  - `lib/api/*` client modules
  - Fetch or KY wrapper (`apiClient.ts`)
  - TanStack Query for caching/querying

### Authentication Model
- ASP.NET issues:
  - **HttpOnly** cookies
  - Optional **Refresh cookie**
- Front-end authenticated requests use:
  ```ts
  fetch(url, { credentials: "include" })
  ```
- Front-end never stores tokens in localStorage.

### Mailgun
- Only the backend talks to Mailgun.
- Front-end triggers notifications via REST endpoints (ex: `/notifications/welcome`).

---

## 3. Project Structure

```
src/
  app/
    App.tsx
    routes.tsx

  pages/
    dashboard/
      DashboardPage.tsx
    profile/
      ProfilePage.tsx
    login/
      LoginPage.tsx

  components/
    ui/                   # shadcn/ui atoms
    layout/               # Header, Sidebar, AppShell
    common/               # utilities, modals, etc.

  lib/
    api/
      apiClient.ts       # base fetch wrapper w/ credentials
      auth.api.ts        # login/logout/me
      users.api.ts       # user CRUD
      ...                # add new resource-specific API modules
    auth/
      useAuth.ts         # auth state, “isAuthenticated”
      AuthProvider.tsx   # wrapper for protected routes

  types/
    api/
      auth.ts            # DTOs for login, user profile
      user.ts
      ...

  hooks/
    useDebounce.ts
    usePagination.ts
    ...

  styles/
    globals.css
    tailwind.css

main.tsx
index.html
```

---

## 4. API Client Pattern

### `apiClient.ts`

```ts
const API_URL = import.meta.env.VITE_API_URL;

export async function apiGet<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    }
  });

  if (!res.ok) {
    throw new Error(`API error: ${res.status}`);
  }

  return res.json() as Promise<T>;
}

export async function apiPost<T>(path: string, body: any, options?: RequestInit): Promise<T> {
  return apiGet<T>(path, {
    method: "POST",
    body: JSON.stringify(body),
    ...options
  });
}
```

### Example resource client

```ts
// lib/api/users.api.ts
import { apiGet } from "./apiClient";
import type { UserProfile } from "@/types/api/user";

export function getCurrentUser(): Promise<UserProfile> {
  return apiGet("/users/me");
}
```

---

## 5. TanStack Query Pattern

### Basic usage

```ts
import { useQuery } from "@tanstack/react-query";
import { getCurrentUser } from "@/lib/api/users.api";

export function useCurrentUser() {
  return useQuery({
    queryKey: ["currentUser"],
    queryFn: getCurrentUser
  });
}
```

### Mutation example

```ts
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateProfile } from "@/lib/api/users.api";

export function useUpdateProfile() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: updateProfile,
    onSuccess: () => qc.invalidateQueries(["currentUser"])
  });
}
```

---

## 6. Routing Pattern (React Router)

```tsx
import { createBrowserRouter } from "react-router-dom";
import AppLayout from "@/components/layout/AppLayout";
import LoginPage from "@/pages/login/LoginPage";
import DashboardPage from "@/pages/dashboard/DashboardPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [
      { path: "dashboard", element: <DashboardPage /> },
      { path: "profile", element: <ProfilePage /> }
    ]
  },
  { path: "/login", element: <LoginPage /> }
]);
```

---

## 7. Shared UI Pattern (shadcn/ui)

```tsx
import { Button } from "@/components/ui/button";

export function SaveButton() {
  return <Button variant="default">Save</Button>;
}
```

---

## 8. Auth Flow (AI-Friendly)

### Client-side auth wrapper

```tsx
export function RequireAuth({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, loading } = useAuth();

  if (loading) return <div>Loading...</div>;
  if (!isAuthenticated) return <Navigate to="/login" />;

  return <>{children}</>;
}
```

### Usage

```tsx
<Route
  path="dashboard"
  element={
    <RequireAuth>
      <DashboardPage />
    </RequireAuth>
  }
/>
```

---

## 9. AI Agent Usage Guidelines

### 9.1. General Rules for AI Agents

1. **Always generate code inside existing structure**  
2. **Use TypeScript strictly.**
3. **Use TanStack Query for any server data.**
4. **Never store tokens in localStorage.**
5. **Follow the API client pattern.**
6. **UI components must use shadcn/ui or Tailwind.**
7. **Forms must use React Hook Form + Zod.**

---

### 9.2. Example AI Tasks (Prompts)

#### Generate a new API client
> “Add API client functions for `/orders` resource...”

#### Generate a dashboard page
> “Create a new page `OrdersPage.tsx`...”

#### Add a protected route
> “Wrap the new `/orders` route...”

#### Create a form
> “Generate a Profile Update form...”

#### Integrate mail
> “Generate a function `sendWelcomeEmail(userId)`...”

---

## 10. Conventions
- Pages: `XxxPage.tsx`
- API clients: `xxx.api.ts`
- DTO types: `types/api/xxx.ts`
- Hooks start with `useXxx`
- Functional components only

---

## 11. Summary
This document defines the full architecture for an AI-driven React SPA front-end, with predictable structure, consistent conventions, and safe patterns for AI-powered code generation.
