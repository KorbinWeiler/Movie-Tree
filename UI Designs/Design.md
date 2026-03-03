# Movie Tree — Design Specification

> **Stack context:** Nuxt · Vue · Vuetify · TailwindCSS · TypeScript  
> **Theme:** Dark-first UI  
> **Last updated:** 2026-03-03

---

## Table of Contents

1. [Color Palette](#1-color-palette)
2. [Typography](#2-typography)
3. [Spacing & Layout](#3-spacing--layout)
4. [Elevation & Shadows](#4-elevation--shadows)
5. [Border Radius](#5-border-radius)
6. [Iconography](#6-iconography)
7. [Components](#7-components)
   - 7.1 Buttons
   - 7.2 Cards
   - 7.3 Inputs & Search
   - 7.4 Rating Widget
   - 7.5 Chips & Tags
   - 7.6 Navigation
   - 7.7 Modals & Sheets
   - 7.8 Toasts & Alerts
8. [Motion & Animation](#8-motion--animation)
9. [Responsive Breakpoints](#9-responsive-breakpoints)
10. [Page-Specific Guidelines](#10-page-specific-guidelines)

---

## 1. Color Palette

### 1.1 Brand Colors

| Token | Hex | Usage |
|---|---|---|
| `primary` | `#FFD147` | CTAs, active states, highlights, star ratings |
| `primary-hover` | `#FFE070` | Hover state of primary elements |
| `primary-pressed` | `#E6B800` | Active/pressed state |
| `primary-subtle` | `#FFD14720` | Tinted backgrounds (e.g. selected pill) |

> **Dandelion Yellow** `#FFD147` — warm, saturated yellow that reads clearly on dark surfaces without feeling neon.

### 1.2 Neutral / Background Scale

| Token | Hex | Usage |
|---|---|---|
| `bg-base` | `#0F1117` | Page / app background |
| `bg-surface` | `#1A1D25` | Cards, panels, sheets |
| `bg-elevated` | `#22263000` | Modals, dropdowns, popovers |
| `bg-elevated` | `#232730` | Modals, dropdowns, popovers |
| `bg-overlay` | `#00000080` | Dimming overlay (50% black) |
| `border` | `#2C3040` | Default border / divider |
| `border-subtle` | `#1F2230` | Subtle section dividers |

### 1.3 Text

| Token | Hex | Usage |
|---|---|---|
| `text-primary` | `#F0F1F5` | Body copy, headings |
| `text-secondary` | `#9BA0B0` | Supporting labels, metadata |
| `text-muted` | `#565A6E` | Placeholders, disabled text |
| `text-on-primary` | `#111318` | Text placed on `primary` background |

### 1.4 Semantic Colors

| Token | Hex | Usage |
|---|---|---|
| `success` | `#4DB87A` | Watch-later added, friend added |
| `error` | `#E05656` | Errors, delete confirmations |
| `warning` | `#F79B3E` | Warnings, nudges |
| `info` | `#5B8FD4` | Informational states |

### 1.5 Star Rating Gradient

Star ratings use a three-stop fill gradient based on the score:

| Score Range | Color |
|---|---|
| 1–3 | `#E05656` (red) |
| 4–6 | `#F79B3E` (orange) |
| 7–10 | `#FFD147` (primary yellow) |

---

## 2. Typography

**Primary Font:** `Inter` (Google Fonts)  
**Fallback stack:** `system-ui, -apple-system, sans-serif`  
**Monospace (for scores/numbers):** `"Inter Tight", monospace`

### 2.1 Type Scale

| Role | Size | Weight | Line Height | Letter Spacing |
|---|---|---|---|---|
| Display | `2.5rem` (40px) | 700 Bold | 1.15 | -0.02em |
| Heading 1 | `2rem` (32px) | 700 Bold | 1.2 | -0.01em |
| Heading 2 | `1.5rem` (24px) | 600 SemiBold | 1.25 | -0.01em |
| Heading 3 | `1.25rem` (20px) | 600 SemiBold | 1.3 | 0 |
| Heading 4 | `1rem` (16px) | 600 SemiBold | 1.4 | 0 |
| Body Large | `1rem` (16px) | 400 Regular | 1.6 | 0 |
| Body | `0.875rem` (14px) | 400 Regular | 1.6 | 0 |
| Caption | `0.75rem` (12px) | 400 Regular | 1.5 | 0.01em |
| Label | `0.75rem` (12px) | 500 Medium | 1.4 | 0.04em uppercase |
| Score (large) | `2rem` (32px) | 700 Bold | 1 | -0.02em |

### 2.2 Rules

- **Never** use pure white (`#FFFFFF`) for body text; use `text-primary` (`#F0F1F5`) to reduce eye strain.
- Movie titles should always use **Heading 3** or above.
- All caps is reserved for `Label` tokens only (e.g., section headers like "TRENDING NOW").

---

## 3. Spacing & Layout

Uses a **4px base unit**. All spacing values are multiples of 4.

### 3.1 Spacing Scale

| Token | Value | Tailwind Class |
|---|---|---|
| `space-1` | 4px | `p-1` / `m-1` |
| `space-2` | 8px | `p-2` / `m-2` |
| `space-3` | 12px | `p-3` / `m-3` |
| `space-4` | 16px | `p-4` / `m-4` |
| `space-5` | 20px | `p-5` / `m-5` |
| `space-6` | 24px | `p-6` / `m-6` |
| `space-8` | 32px | `p-8` / `m-8` |
| `space-10` | 40px | `p-10` / `m-10` |
| `space-12` | 48px | `p-12` / `m-12` |
| `space-16` | 64px | `p-16` / `m-16` |

### 3.2 Layout Structure

```
┌─────────────────────────────────────────────────────┐
│              Top Navigation Bar (64px)              │
├──────────────┬──────────────────────────────────────┤
│              │                                      │
│   Side Nav   │          Content Area                │
│   (240px)    │   max-width: 1280px, centered        │
│              │   padding: 24px (desktop)            │
│              │   padding: 16px (mobile)             │
│              │                                      │
└──────────────┴──────────────────────────────────────┘
```

- **Page max-width:** `1280px`
- **Content horizontal padding:** `24px` desktop / `16px` mobile
- **Section vertical gap:** `48px` between major page sections
- **Card grid gap:** `16px` (mobile) / `24px` (desktop)
- **Side nav width:** `240px` (collapsed: `64px`)

### 3.3 Grid System

| Layout | Columns | Gutter |
|---|---|---|
| Mobile (< 640px) | 2 | 12px |
| Tablet (640–1024px) | 3–4 | 16px |
| Desktop (> 1024px) | 5–6 | 24px |

Movie poster cards maintain a **2:3 aspect ratio** (width:height).

---

## 4. Elevation & Shadows

Dark theme shadows use a slightly tinted dark color rather than pure black.

| Level | Usage | CSS |
|---|---|---|
| 0 — Flat | Default surface | `none` |
| 1 — Raised | Cards at rest | `0 2px 8px rgba(0,0,0,0.45)` |
| 2 — Floating | Cards on hover, dropdowns | `0 8px 24px rgba(0,0,0,0.55)` |
| 3 — Modal | Modals, bottom sheets | `0 16px 48px rgba(0,0,0,0.65)` |

Primary-glow (used on focused/active primary elements):  
`0 0 0 3px rgba(255, 209, 71, 0.30)`

---

## 5. Border Radius

| Token | Value | Usage |
|---|---|---|
| `rounded-sm` | `4px` | Chips, small badges |
| `rounded` | `8px` | Inputs, small cards |
| `rounded-md` | `12px` | Standard cards |
| `rounded-lg` | `16px` | Movie poster cards |
| `rounded-xl` | `20px` | Modals, bottom sheets |
| `rounded-full` | `9999px` | Avatars, pill buttons, tags |

---

## 6. Iconography

- **Library:** Material Design Icons (via Vuetify's default icon set)
- **Sizes:** `16px` inline / `20px` standard / `24px` toolbar / `32px` feature
- **Color:** Inherit from surrounding text color by default; use `primary` for star/rating icons.
- Icons should never appear without an accessible `aria-label` or adjacent text label.

---

## 7. Components

### 7.1 Buttons

#### Primary Button
- Background: `primary` (`#FFD147`)
- Text: `text-on-primary` (`#111318`), **600 SemiBold**, `14px`
- Padding: `10px 20px`
- Border radius: `rounded-full` (`9999px`)
- Hover: background `primary-hover`, shadow Level 1
- Active: background `primary-pressed`
- Disabled: `bg-surface`, `text-muted`, `cursor-not-allowed`

#### Secondary Button
- Background: transparent
- Border: `1px solid border` (`#2C3040`)
- Text: `text-primary`, **500 Medium**, `14px`
- Hover: border `primary`, text `primary`, background `primary-subtle`

#### Ghost / Icon Button
- Background: transparent
- Icon color: `text-secondary`
- Hover: background `bg-surface`, icon `text-primary`
- Size: `36px × 36px` minimum tap target

#### Danger Button
- Background: `error` (`#E05656`)
- Text: `#F0F1F5`
- Use only for destructive, irreversible actions

#### Button Sizes

| Size | Height | Padding H | Font |
|---|---|---|---|
| Small | 32px | 12px | 12px |
| Medium (default) | 40px | 20px | 14px |
| Large | 48px | 24px | 16px |

---

### 7.2 Cards

#### Movie Poster Card
- Aspect ratio: `2:3`
- Border radius: `rounded-lg` (16px)
- Overflow: hidden
- At rest: shadow Level 1
- On hover: scale `1.03`, shadow Level 2, transition `200ms ease`
- Footer overlay: gradient `linear-gradient(to top, rgba(0,0,0,0.85) 0%, transparent 50%)`
- Footer contains: title (Body Bold), year (Caption, `text-secondary`)

#### Review Card
- Background: `bg-surface`
- Border: `1px solid border`
- Border radius: `rounded-md` (12px)
- Padding: `16px`
- Contains: user avatar (32px circle), username (Label), score badge, review text, date caption

#### Feature Card (home page highlight)
- Full-width banner with blurred backdrop of movie
- Overlay: `linear-gradient(to right, rgba(15,17,23,0.95) 30%, transparent 100%)`
- Contains: title (Heading 1), tagline (Body Large, `text-secondary`), genre chips, CTA buttons

---

### 7.3 Inputs & Search

#### Text Input
- Height: `44px`
- Background: `bg-surface`
- Border: `1px solid border`
- Border radius: `rounded` (8px)
- Padding: `10px 14px`
- Font: Body, `text-primary`
- Placeholder: `text-muted`
- Focus: border `primary`, shadow `primary-glow`

#### Search Bar
- Height: `48px`
- Border radius: `rounded-full`
- Left icon: search icon, `20px`, `text-secondary`
- Background: `bg-surface`
- Focus expands width on desktop (from `280px` → `400px`, transition `250ms`)

#### Select / Dropdown
- Matches text input styling
- Trailing chevron icon: `text-secondary`
- Dropdown panel: `bg-elevated`, `border`, shadow Level 2, `rounded-md`
- Option hover state: background `primary-subtle`, text `primary`

---

### 7.4 Rating Widget

- **10-star display** (filled/half/empty)
- Filled star color: `primary` (`#FFD147`)
- Empty star color: `text-muted`
- Star size: `20px` (inline), `28px` (rating modal), `14px` (card overlay)
- Large numeric score uses **Score** type token (32px, bold)
- Rating badge (compact): `primary` background pill, `text-on-primary`, bold

---

### 7.5 Chips & Tags

#### Genre Chip
- Background: `bg-elevated`
- Border: `1px solid border`
- Border radius: `rounded-sm` (4px)
- Padding: `4px 10px`
- Font: Caption, `text-secondary`
- Active/selected: background `primary-subtle`, border `primary`, text `primary`

#### Status Pill (watch later, watched)
- Border radius: `rounded-full`
- Padding: `4px 12px`
- "Watch Later": background `info` at 15% opacity, text `info`
- "Watched": background `success` at 15% opacity, text `success`

---

### 7.6 Navigation

#### Top Navigation Bar
- Height: `64px`
- Background: `bg-base` with `border-bottom: 1px solid border-subtle`
- Logo: Heading 3, `primary` color, left-most
- Nav links: Body, `text-secondary`; active link: `text-primary`, bottom border `2px solid primary`
- Right: search icon, notifications bell, user avatar

#### Side Navigation (desktop)
- Width: `240px` (expanded), `64px` (collapsed)
- Background: `bg-surface`
- Border-right: `1px solid border`
- Nav items: icon + label, padding `12px 16px`, border-radius `rounded`
- Active item: background `primary-subtle`, text `primary`, icon `primary`
- Collapse toggle: bottom of panel, ghost icon button

#### Bottom Navigation (mobile, ≤ 640px)
- Height: `56px`
- Background: `bg-surface`
- Border-top: `1px solid border`
- 4–5 icon-only items equally spaced
- Active: icon `primary`, small dot indicator below

---

### 7.7 Modals & Sheets

- Overlay: `bg-overlay` (`rgba(0,0,0,0.5)`)
- Modal background: `bg-elevated`, border `border`, shadow Level 3
- Border radius: `rounded-xl` (20px)
- Max-width: `560px` (standard) / `800px` (wide)
- Padding: `24px`
- Close button: top-right, ghost icon button
- Enter/leave: fade + translateY(12px) scale(0.98), `250ms ease`

#### Bottom Sheet (mobile)
- Attaches to screen bottom
- Border radius: top corners `20px` only
- Drag handle: `32px × 4px`, `border` color, centered, `8px` from top
- Background: `bg-elevated`

---

### 7.8 Toasts & Alerts

#### Toast Notification
- Position: bottom-right, `24px` from edge
- Background: `bg-elevated`
- Border-left: `4px solid` (semantic color: success/error/info/warning)
- Border radius: `rounded` (8px)
- Shadow: Level 2
- Body: `text-primary`, `14px`; optional `text-secondary` subtext
- Auto-dismiss: `4000ms`; progress bar in semantic color

#### Inline Alert
- Background: semantic color at 10% opacity
- Border: `1px solid` semantic color at 40% opacity
- Border radius: `rounded` (8px)
- Padding: `12px 16px`
- Icon: `20px`, semantic color

---

## 8. Motion & Animation

All transitions should feel **snappy but not jarring**. Prefer short durations.

| Type | Duration | Easing |
|---|---|---|
| Micro (hover, focus) | `150ms` | `ease` |
| Standard (state change, expand) | `200–250ms` | `ease` |
| Page transition | `300ms` | `cubic-bezier(0.4, 0, 0.2, 1)` |
| Modal enter/leave | `250ms` | `ease` |
| Skeleton shimmer | `1500ms` | `linear` loop |

### Skeleton Loading
- Background: `bg-surface`
- Shimmer: gradient sweep from `bg-surface` → `bg-elevated` → `bg-surface`
- Use for: movie cards, review cards, hero section
- Match exact shape/size of the content it replaces

### Hover Effects
- Movie poster cards: `scale(1.03)` + shadow elevation increase
- Buttons: 10% background lightening, `150ms`
- Links: color shift to `primary`, `150ms`

### Reduced Motion
- Respect `prefers-reduced-motion: reduce` — disable scale and translate animations; keep opacity fade only.

---

## 9. Responsive Breakpoints

Matches Tailwind CSS and Vuetify defaults.

| Name | Min Width | Key Layout Changes |
|---|---|---|
| `xs` | 0px | Single column, bottom navigation |
| `sm` | 640px | 2 columns, hide some labels |
| `md` | 768px | Side nav appears (collapsed), 3 columns |
| `lg` | 1024px | Side nav expanded, 4–5 columns |
| `xl` | 1280px | Max content width reached |
| `2xl` | 1536px | Max-width enforced, more whitespace |

---

## 10. Page-Specific Guidelines

### Home Page
- **Hero Section:** Full-width feature card (one AI or trending pick), `400px` height desktop / `280px` mobile
- **Section Headers:** Label token (all caps), `text-secondary`, paired with a "See All" ghost link (`primary`)
- **Row scroll:** Horizontal scroll on mobile for "Trending" and "Watch Later" rows with scroll snap
- **AI Picks section:** Subtle `primary-subtle` background tint to distinguish it visually
- **Section spacing:** `48px` vertical gap between rows

### Search Page
- Sticky search bar at top
- Results grid follows the standard column system
- No-results state: centered illustration, Heading 3, Body subtext, suggested genres as chips
- Filter bar: horizontal chip row below search bar (Genre, Year, Rating); active filters show count badge

### Feed Page
- Two tabs: **Public** / **Friends** — tab underline indicator `primary`, `2px`
- Review cards in a single-column feed, max-width `680px`, centered
- Infinite scroll with skeleton loaders at bottom
- Friend reviews show avatar + username inline above movie title

### Generate Page
- Step-by-step wizard or settings panel on the left + results on the right (desktop)
- Mode selector: large radio cards (icon + title + description), selected state uses `primary` border and `primary-subtle` bg
- "Generate" CTA: Large Primary Button, full-width on mobile
- Generated list: numbered `1–10`, poster card + title + brief reason text from AI

### Settings Page
- Left section menu (becomes accordion on mobile)
- Form fields in groups with `border-subtle` dividers
- Destructive actions (delete account) placed last, styled as Danger Button
- Privacy options for reviews: pill toggle group (Public / Friends / Private)
