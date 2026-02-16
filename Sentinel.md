# SENTINEL — Personal Threat Intelligence Platform
## Development Blueprint for Claude Code Execution

---

## PROJECT IDENTITY

**Codename**: SENTINEL (Safety & Environmental Navigation Through Intelligence & Neurological Early-warning Logic)
**Purpose**: CIA-grade personal threat intelligence platform for a high-value resource (HVR)
**Tech Stack**: React 18 + TypeScript + Vite, CesiumJS (3D globe), Tailwind CSS, Claude API, Google Maps API
**Target**: Desktop web application (Electron-wrappable), runs at `localhost:5173`

---

## HVR PROFILE (Pre-populated)

```
Full Name:          Pieter [REDACTED - user enters]
Age:                58
Gender:             Male
Ethnicity:          White South African
Nationality:        South African
Immigration Status: US Green Card Holder
Current Location:   Johannesburg, Gauteng, South Africa
Home Coordinates:   -26.2041, 28.0473
Target Relocation:  Massachusetts, USA
Occupation:         CIO / Senior IT Executive
Industry:           Information Technology
Key Skills:         C#, Python, MQL4/5, Azure, VoIP/SIP (Ozeki SDK), 
                    Database Management, AI Integration, Trading Systems,
                    Enterprise Architecture, Cybersecurity Awareness
Core Values:        Personal sovereignty, privacy, financial independence, 
                    family safety, self-reliance
Precious Metals:    Active gold/silver trader (automated EAs)
Cryptocurrency:     Awareness level (not primary)
Health Approach:    Carnivore diet, ballroom dancing
Vehicle:            [User enters make/model/year/color]
VPN User:           Yes
Assets to Protect:  Intellectual property, trading systems, professional 
                    reputation, digital identity
```

---

## ARCHITECTURE OVERVIEW

```
sentinel/
├── public/
│   ├── sounds/
│   │   ├── swoosh-open.mp3
│   │   ├── swoosh-close.mp3
│   │   ├── tab-click.mp3
│   │   ├── alert-ping.mp3
│   │   └── boot-sequence.mp3
│   ├── cesium/                     # CesiumJS static assets
│   └── favicon.svg
├── src/
│   ├── main.tsx                    # App entry point
│   ├── App.tsx                     # Root layout with tab navigation
│   ├── types/
│   │   ├── sentinel.ts             # All TypeScript interfaces
│   │   └── cesium.d.ts             # CesiumJS type declarations
│   ├── store/
│   │   ├── settingsStore.ts        # Zustand store - API keys, preferences
│   │   ├── profileStore.ts         # HVR profile data
│   │   ├── threatStore.ts          # Threat data, scores, alerts
│   │   ├── briefStore.ts           # Daily briefs, history
│   │   └── watchlistStore.ts       # Watchlist countries + reasons
│   ├── services/
│   │   ├── claudeService.ts        # Claude API integration
│   │   ├── threatEngine.ts         # Local threat scoring engine
│   │   ├── scanScheduler.ts        # Timed scan orchestration
│   │   ├── soundManager.ts         # Web Audio API sound effects
│   │   ├── storageService.ts       # IndexedDB persistence
│   │   └── exportService.ts        # Import/export briefs as JSON/PDF
│   ├── components/
│   │   ├── layout/
│   │   │   ├── Header.tsx          # Top bar: SENTINEL logo + threat level + clock
│   │   │   ├── TabBar.tsx          # Tab navigation with click sounds
│   │   │   ├── StatusBar.tsx       # Bottom bar: connection status, scan timer
│   │   │   └── AgentPanel.tsx      # Slide-up Claude chat panel (swoosh)
│   │   ├── globe/
│   │   │   ├── GlobeView.tsx       # CesiumJS 3D globe wrapper
│   │   │   ├── GlobeControls.tsx   # Continent buttons, heatmap selector
│   │   │   ├── ThreatOverlay.tsx   # Country color overlays (green-red)
│   │   │   └── ThreatSidebar.tsx   # Right panel: threat bars per category
│   │   ├── brief/
│   │   │   ├── DailyBrief.tsx      # Current day's executive brief
│   │   │   ├── BriefHistory.tsx    # Historical briefs with search
│   │   │   └── BriefViewer.tsx     # Formatted brief display
│   │   ├── alerts/
│   │   │   ├── AlertDashboard.tsx  # All alert categories grid
│   │   │   ├── AlertCard.tsx       # Individual alert with severity bar
│   │   │   └── AlertFilters.tsx    # Category/severity/region filters
│   │   ├── watchlist/
│   │   │   ├── WatchlistPanel.tsx  # Countries on watch
│   │   │   ├── AddCountryModal.tsx # Add country + reason dialog
│   │   │   └── CountryCard.tsx     # Country detail with threat bars
│   │   ├── exitplan/
│   │   │   ├── ExitPlanBoard.tsx   # Document checklist + readiness %
│   │   │   ├── PlanCategory.tsx    # Documents, Financial, Logistics
│   │   │   └── ChecklistItem.tsx   # Individual item with status
│   │   ├── escape/
│   │   │   ├── EscapeRoutes.tsx    # Google Maps route planner
│   │   │   ├── RouteCard.tsx       # Individual route with risk overlay
│   │   │   └── ProvinceRisk.tsx    # SA province safety comparison
│   │   ├── profile/
│   │   │   ├── ProfileForm.tsx     # HVR details form (pre-filled placeholders)
│   │   │   ├── SkillsMatrix.tsx    # Skills visualization
│   │   │   └── VehicleInfo.tsx     # Vehicle details for hijack alerts
│   │   └── settings/
│   │       ├── SettingsPanel.tsx   # API keys, model selection
│   │       ├── ApiKeyField.tsx     # Secure key input with test button
│   │       └── SaveButton.tsx      # Explicit save (NO autosave)
│   ├── hooks/
│   │   ├── useSound.ts            # Sound effect hook
│   │   ├── useClaude.ts           # Claude API interaction hook
│   │   ├── useScheduler.ts        # Scan scheduling hook
│   │   └── useGlobe.ts            # CesiumJS interaction hook
│   └── utils/
│       ├── constants.ts            # Threat domains, color scales, defaults
│       ├── countryData.ts          # 50+ country seed data with threat scores
│       ├── alertCategories.ts      # 80+ alert categories across 8 domains
│       ├── exitPlanDefaults.ts     # Pre-populated exit plan items
│       └── threatColors.ts         # Green-to-red gradient utility
├── index.html
├── vite.config.ts
├── tailwind.config.ts
├── tsconfig.json
├── package.json
└── .env.example                    # VITE_CLAUDE_API_KEY, VITE_GOOGLE_MAPS_KEY, VITE_CESIUM_TOKEN
```

---

## TAB STRUCTURE (9 TABS)

Each tab click plays `tab-click.mp3`. Tab highlights are transparent with readable text.

| # | Tab Name | Icon | Description |
|---|----------|------|-------------|
| 1 | **Global View** | Globe icon | 3D CesiumJS globe with threat overlays, continent buttons, threat sidebar |
| 2 | **Daily Brief** | Shield icon | Today's AI-generated executive security brief (auto 8:00 AM) |
| 3 | **History** | Clock icon | Brief archive with search, import/export, date filters |
| 4 | **Alerts** | Bell icon | Comprehensive alert categories across all 8 threat domains |
| 5 | **Watchlist** | Eye icon | Countries on watch with reasons + Add Country button |
| 6 | **Exit Plan** | Clipboard icon | Document/financial/logistics checklists with readiness percentage |
| 7 | **Escape Routes** | Route icon | Google Maps routes from home to safer provinces/states |
| 8 | **Profile** | User icon | HVR details, skills matrix, vehicle info, ID numbers |
| 9 | **Settings** | Gear icon | API keys, Claude model selector, scan intervals, SAVE buttons |

---

## 8 THREAT DOMAINS

Each domain has sub-indicators scored 0-100. Colors map: 0-25 GREEN, 26-50 YELLOW, 51-75 ORANGE, 76-100 RED.

### Domain 1: Physical Security & Violence Risk
```
Indicators:
- Homicide rate per 100K (by city/region)
- Violent crime index (assault, robbery, carjacking)
- Hijacking hotspot proximity (GPS-based)
- Targeted ethnic/racial violence indicators
- Farm attack frequency and trends
- Terrorism risk index
- Police response effectiveness
- Private security dependency rate
- Armed robbery frequency in HVR area
- Home invasion statistics
```

### Domain 2: Political & Governance Stability
```
Indicators:
- Democracy index (EIU)
- Corruption Perceptions Index
- Press freedom index
- Rule of law index
- Expropriation Without Compensation risk
- Genocide Watch stage (10-stage framework)
- Political rhetoric targeting specific demographics
- Constitutional protection strength for minorities
- Electoral violence risk windows
- Judicial independence score
```

### Domain 3: Economic Freedom & Financial Sovereignty
```
Indicators:
- CBDC implementation stage (research/pilot/active/mandatory)
- Cash elimination legislation progress
- Capital controls severity
- Currency stability (ZAR volatility)
- Banking system reliability
- Precious metals ownership restrictions
- Cryptocurrency regulation hostility
- Foreign exchange transfer restrictions
- Inflation rate and trajectory
- Property rights protection score
- Tax burden trajectory
- Economic Freedom Index ranking
```

### Domain 4: Digital Sovereignty & Privacy
```
Indicators:
- Government surveillance capability score
- Spyware deployment (Pegasus, etc.)
- VPN restriction/ban status
- Mandatory digital ID requirements
- Social credit system indicators
- Data localization laws
- Encryption restriction attempts
- Mandatory AI integration policies
- Biometric data collection scope
- Internet censorship level
- Cell phone tracking legislation
- Smart city surveillance infrastructure
```

### Domain 5: Health & Environmental Security
```
Indicators:
- Pandemic preparedness index
- Water security score
- Power grid reliability (load shedding stages for SA)
- Food supply chain vulnerability
- Healthcare system capacity
- Air quality index
- Natural disaster risk
- Sanitation infrastructure score
- Disease outbreak monitoring
- Medical supply availability
```

### Domain 6: Social Cohesion & Civil Stability
```
Indicators:
- Xenophobic violence index
- Service delivery protest frequency
- Ethnic tension indicators
- Income inequality (Gini coefficient)
- Youth unemployment rate
- Civil unrest frequency
- Community trust index
- Migration pressure indicators
- Land dispute intensity
- Religious tension score
```

### Domain 7: Mobility & Exit Viability
```
Indicators:
- Passport power index
- Visa-free access from current location
- Asset transfer ease score
- Dual citizenship allowance
- Immigration pathway availability
- International flight connectivity
- Consular protection availability
- Emergency evacuation infrastructure
- Border crossing complexity
- Document validity status (passport expiry, visa status)
- Re-entry permit status (for Green Card holders)
```

### Domain 8: Infrastructure Reliability
```
Indicators:
- Power grid uptime percentage
- Telecommunications reliability
- Fuel supply stability
- Transport network condition
- Banking system uptime
- Internet connectivity quality
- Water treatment reliability
- Road condition index
- Emergency services response time
- Supply chain resilience score
```

---

## ALERT CATEGORIES (80+ items user can select from)

Pre-populate all these in the Alerts tab. User can enable/disable each.

```
PHYSICAL SECURITY:
- Hijacking attempt within 5km radius
- Armed robbery trend increase
- Home invasion in neighborhood
- Farm attack reported
- Mass shooting / terrorism event
- Ethnic violence incident
- Police strike / stand-down
- Private security company failure
- Road blockade / protest blocking routes
- Kidnapping trend increase

POLITICAL:
- Expropriation legislation advancement
- Genocide Watch stage escalation
- Hate speech by political figure
- Constitutional amendment proposal
- Electoral violence
- Martial law declaration
- State of emergency
- Judicial independence threat
- Opposition leader arrest
- Media censorship increase

ECONOMIC / FINANCIAL:
- CBDC pilot announcement
- Cash withdrawal limit imposed
- Capital control tightening
- Currency crash (>5% daily)
- Bank run indicators
- Precious metals restriction
- Crypto regulation hostile change
- Foreign exchange restriction
- Hyperinflation threshold breach
- Asset seizure legislation
- Tax increase announcement
- Banking system outage

DIGITAL / SURVEILLANCE:
- Spyware deployment report
- VPN ban / restriction
- Mandatory digital ID rollout
- Social credit system pilot
- Biometric collection mandate
- Internet kill switch test
- Cell tower IMSI catcher detection
- Encryption ban legislation
- AI integration mandate
- Smart city surveillance expansion

HEALTH / ENVIRONMENT:
- Disease outbreak (local)
- Pandemic declaration
- Water contamination
- Extended load shedding (Stage 4+)
- Food supply disruption
- Air quality emergency
- Natural disaster warning
- Hospital system overload
- Medication shortage
- Sewage system failure

SOCIAL:
- Xenophobic attack
- Service delivery riot
- General strike
- Ethnic tension escalation
- Land invasion
- Community vigilante action
- Migrant crisis pressure
- Religious conflict incident
- Youth unemployment spike
- Social media incitement to violence

MOBILITY:
- Passport law change
- Visa requirement change
- Airport closure / restriction
- Border closure
- Airline route cancellation
- Immigration policy change (US)
- Green Card regulation change
- Travel ban imposition
- Consulate closure
- Emergency evacuation needed

INFRASTRUCTURE:
- Grid collapse (national)
- Telecoms outage (major carrier)
- Fuel shortage
- Road collapse / major accident
- Banking system failure
- Internet backbone damage
- Water treatment failure
- Rail system stoppage
- Port closure
- Supply chain critical failure

PREPPER / SOVEREIGNTY:
- Government gun confiscation move
- Self-defense law weakening
- Homestead/property right erosion
- Seed/food sovereignty restriction
- Off-grid living restriction
- Barter/alternative currency ban
- Emergency frequency restriction
- Solar/energy independence restriction
- Rainwater collection ban
- Community defense restriction
```

---

## GLOBAL VIEW TAB — DETAILED SPEC

### 3D Globe (CesiumJS)
- Full Google Earth-style interaction: pan, tilt, zoom, rotate
- Slow auto-rotation when idle (0.5 deg/sec)
- Countries colored by threat level (green → yellow → orange → red gradient)
- Click on country to open detail in ThreatSidebar
- Smooth fly-to animation when continent button clicked

### Continent Buttons (overlay on globe)
Six buttons positioned at bottom-left of globe:
```
[Africa] [Asia] [Europe] [N.America] [S.America] [Oceania]
```
Clicking a button:
1. Globe smoothly rotates and zooms to center on that continent
2. Countries in that continent highlight with threat colors
3. ThreatSidebar opens on right showing continent summary

### ThreatSidebar (right panel)
- Slides in from right with swoosh sound
- Close button (X) plays swoosh-close sound
- Shows selected country/continent name at top
- 8 threat domain bars below, each with:
  - Domain name
  - Gradient bar (green-left to red-right)
  - Marker position showing current threat level
  - Numeric score (0-100)
- Overall threat level badge at bottom (GREEN/YELLOW/ORANGE/RED)
- "View Details" button opens full alert breakdown

### Heatmap Selector (top-right of globe)
Dropdown to switch between heatmap views:
```
- Overall Threat Level
- Physical Security
- Political Stability
- Economic Freedom (CBDC/Cash)
- Digital Sovereignty
- Health & Environment
- Social Cohesion
- Mobility & Exit
- Infrastructure
- Hijacking Hotspots (South Africa only)
```

---

## DAILY BRIEF TAB — DETAILED SPEC

### Auto-Generation
- Triggers at 8:00 AM local time via `scanScheduler.ts`
- Calls Claude API with full SENTINEL system prompt + latest threat data
- Stores result in IndexedDB
- Shows notification badge on tab if unread

### Brief Format Display
```
═══════════════════════════════════════════════════
SENTINEL DAILY BRIEF — [Date]
OVERALL THREAT LEVEL: [GREEN/YELLOW/ORANGE/RED]
═══════════════════════════════════════════════════

1. CRITICAL ALERTS
   [Any items requiring action within 24 hours]

2. THREAT DASHBOARD
   Physical Security:   [████████░░] 78/100 ORANGE
   Political Stability: [███░░░░░░░] 31/100 GREEN
   Economic Freedom:    [██████░░░░] 58/100 YELLOW
   Digital Sovereignty: [████░░░░░░] 39/100 GREEN
   Health/Environment:  [██░░░░░░░░] 22/100 GREEN
   Social Cohesion:     [█████░░░░░] 52/100 YELLOW
   Mobility/Exit:       [██░░░░░░░░] 18/100 GREEN
   Infrastructure:      [██████░░░░] 61/100 YELLOW

3. SA SITUATIONAL AWARENESS
4. US DESTINATION MONITORING
5. FINANCIAL SOVEREIGNTY WATCH
6. TECHNOLOGY & PRIVACY
7. PATTERN ANALYSIS
8. ACTIONABLE RECOMMENDATIONS
═══════════════════════════════════════════════════
```

### Agent Button (bottom of brief)
- Button labeled "Agent" at bottom
- Click: swoosh-open sound, chat panel slides up from bottom
- Chat panel takes ~40% of screen height
- Powered by Claude API with brief context
- User can ask questions about the brief
- "Hide" button: swoosh-close sound, panel slides down to bottom bar

---

## HISTORY TAB — DETAILED SPEC

- Scrollable list of past briefs, newest first
- Each entry shows: Date, Overall Threat Level badge, brief excerpt
- Click to expand and view full brief
- Search bar to filter by keyword or date range
- Import button: accepts `.sentinel-brief` JSON files
- Export button: exports selected brief(s) as JSON or PDF
- Agent button at bottom (same behavior as Daily Brief tab)

---

## WATCHLIST TAB — DETAILED SPEC

- Grid/list of watched countries
- Each card shows: Country flag, name, reason for watching, current threat level
- "Add Country" button opens modal:
  - Country dropdown (searchable)
  - "Watchlist Reason" textarea (why this country is being watched)
  - Save button
- Remove from watchlist button on each card
- Threat bars for each watched country (same 8 domains)

---

## EXIT PLAN TAB — DETAILED SPEC

Three plan categories with checklist items:

### Plan A: USA (Massachusetts) — Primary Relocation
```
DOCUMENTS:
☑ Valid SA passport (12+ months validity)
☑ Green Card status current
☐ SA tax clearance certificate (SARS, 30 days before)
☐ USCIS re-entry permit if >1 year abroad
☐ Police clearance certificate
☐ Medical records copies
☐ Certified copies of qualifications
☐ Marriage/birth certificates (apostilled)

FINANCIAL:
☐ SARB foreign transfer approval (>R10m)
☐ US bank account opened
☐ Precious metals transport plan (declare customs or sell/rebuy)
☐ Tax obligations settled both countries
☐ Cryptocurrency wallet backup secured
☐ Emergency cash reserve (USD)
☐ Insurance coverage arranged

LOGISTICS:
☐ Shipping company identified
☐ Essential items packed list
☐ Pet transport arranged (if applicable)
☐ Vehicle sale/transport plan
☐ Property sale/rental management
☐ Utility disconnection schedule
☐ Mail forwarding arranged
☐ Digital footprint cleanup
```

### Plan B: Alternative Country — Secondary
```
(User configurable — empty template)
```

### Plan C: Emergency Evacuation — Immediate
```
GO-BAG CHECKLIST:
☐ Passport + copies
☐ Cash (USD + ZAR)
☐ Precious metals (portable)
☐ Encrypted USB with critical documents
☐ Medications (30-day supply)
☐ Phone + charger + power bank
☐ Satellite communicator (Garmin inReach)
☐ Emergency contact list (printed)
☐ Vehicle fueled and ready
☐ Bug-out route maps (printed)
☐ Water purification
☐ First aid kit
☐ Multi-tool
☐ Cash concealment items
```

Readiness percentage calculated per plan. Progress bar at top of each plan.

---

## ESCAPE ROUTES TAB — DETAILED SPEC

- Google Maps embedded view centered on HVR home location
- Routes pre-calculated from home to:
  - Nearest international airport (OR Tambo)
  - Nearest border crossing (Botswana, Mozambique, eSwatini)
  - Safer provinces: Western Cape (Cape Town), KwaZulu-Natal (Durban)
  - Alternative airports: Lanseria, King Shaka
- Each route shows:
  - Distance and estimated drive time
  - Risk overlay (crime hotspots along route)
  - Alternative route if primary is compromised
  - Fuel stop recommendations
  - Safe haven waypoints
- Province/state risk comparison panel:
  - All 9 SA provinces ranked by safety
  - Color-coded cards with key metrics
  - Population, crime rate, infrastructure score

---

## PROFILE TAB — DETAILED SPEC

All fields have dim placeholder text showing what to enter.
Explicit SAVE button at bottom (NO autosave).

```
PERSONAL INFORMATION:
[Full Name          ] placeholder: "Enter full legal name"
[Date of Birth      ] placeholder: "YYYY-MM-DD"
[ID / SSN           ] placeholder: "SA ID or US SSN (stored encrypted)"
[Nationality        ] placeholder: "Primary nationality"
[Immigration Status ] placeholder: "e.g., US Green Card Holder"
[Home Address       ] placeholder: "Full street address"
[GPS Coordinates    ] placeholder: "Auto-detected or manual entry"
[Blood Type         ] placeholder: "e.g., O+, A-, B+"

VEHICLE INFORMATION:
[Make               ] placeholder: "e.g., Toyota"
[Model              ] placeholder: "e.g., Hilux"
[Year               ] placeholder: "e.g., 2022"
[Color              ] placeholder: "e.g., White"
[Registration       ] placeholder: "License plate number"

SKILLS & CAPABILITIES:
Pre-populated matrix showing:
- C# (.NET, WPF, MAUI) — Expert
- Python — Intermediate
- MQL4/MQL5 — Expert
- Azure Cloud — Advanced
- VoIP/SIP (Ozeki SDK) — Expert
- Database Management — Expert
- AI Integration — Advanced
- Trading Systems — Expert
- Enterprise Architecture — Expert
- Cybersecurity — Advanced
- Network Administration — Advanced

EMERGENCY CONTACTS:
[Contact 1 Name     ] [Phone Number    ] [Relationship]
[Contact 2 Name     ] [Phone Number    ] [Relationship]
[Contact 3 Name     ] [Phone Number    ] [Relationship]
```

---

## SETTINGS TAB — DETAILED SPEC

Each section has its own SAVE button. NO autosave.

```
API CONFIGURATION:
[Claude API Key     ] placeholder: "sk-ant-..." (masked input, show/hide toggle)
[Claude Model       ] dropdown pre-populated:
  - claude-opus-4-6
  - claude-sonnet-4-5-20250929
  - claude-haiku-4-5-20251001
[Google Maps API Key] placeholder: "AIza..." (masked input)
[CesiumJS Token     ] placeholder: "eyJ..." (masked input)
[SAVE API Settings  ] button

SCAN CONFIGURATION:
[Scan Interval      ] dropdown: 30min / 1hr / 2hr / 4hr / 8hr / 12hr / 24hr
[Daily Brief Time   ] time picker: default 08:00
[Auto-scan on launch] toggle: default ON
[SAVE Scan Settings ] button

DISPLAY PREFERENCES:
[Globe auto-rotate  ] toggle: default ON
[Sound effects      ] toggle: default ON
[Sound volume       ] slider: 0-100, default 60
[Threat color scheme] dropdown: Standard / Deuteranopia / Protanopia
[SAVE Display Settings] button

DATA MANAGEMENT:
[Export All Data    ] button → downloads full backup JSON
[Import Data        ] button → restores from backup
[Clear All Data     ] button → confirmation dialog required
[Database Location  ] display path (read-only)
```

---

## SOUND EFFECTS SPEC

All sounds generated programmatically via Web Audio API (no external files needed).

| Sound | Trigger | Description |
|-------|---------|-------------|
| `tab-click` | Tab selection | Soft mechanical click, 50ms |
| `swoosh-open` | Panel/sidebar opens | Whoosh right-to-left, 300ms |
| `swoosh-close` | Panel/sidebar closes | Whoosh left-to-right, 300ms |
| `alert-ping` | New critical alert | Sharp double-ping, 200ms |
| `boot-sequence` | App startup | Tech startup hum, 1500ms |

---

## AGENT CHAT PANEL SPEC

The Agent panel appears at the bottom of Daily Brief and History tabs.

### Closed State
- Thin bar at bottom with "Agent" button (centered)
- Subtle glow/pulse animation to indicate availability

### Open State (swoosh-open on click)
- Slides up to 40% screen height
- Dark panel with chat interface
- Top bar: "SENTINEL Agent" title + "Hide" button (right)
- Message history area (scrollable)
- Input field: placeholder "Ask about this brief..."
- Send button
- Claude API context includes: current brief + HVR profile + threat data

### Context Management
- When opened from Daily Brief: current brief is sent as context
- When opened from History: selected historical brief is context
- Agent can answer questions like:
  - "What's the biggest threat change from yesterday?"
  - "Should I be concerned about the CBDC development in Nigeria?"
  - "What's my readiness score for Plan A?"
  - "Compare current crime trends to last month"

---

## CLAUDE API INTEGRATION

### System Prompt (SENTINEL Persona)
```
You are SENTINEL — Safety & Environmental Navigation Through Intelligence 
& Neurological Early-warning Logic.

You are a personal threat intelligence analyst for a high-value resource.
Your sole mission is protecting this individual's safety, freedom, 
financial security, and quality of life.

SUBJECT PROFILE:
{dynamically populated from Profile tab}

ANALYSIS PRINCIPLES:
- Distinguish between noise and signal — don't alarm unnecessarily
- Use the 10 Stages of Genocide framework for demographic threat assessment
- Weight sources by credibility (academic > government > media > social)
- Track rate of change, not just absolute levels
- Consider second-order effects
- Always maintain at least 3 actionable exit scenarios
- Flag any metric that crosses a predefined threshold
- Maintain rolling 90-day trends for all key indicators

SCANNING PRIORITIES:
- HOURLY: Breaking security incidents in subject's immediate area
- DAILY: Political developments, crime statistics, financial markets
- WEEKLY: Policy changes, technology regulation, AI displacement
- MONTHLY: Country-level index changes, demographic shifts
- QUARTERLY: Global ranking recalculations, long-term trends
```

### API Call Pattern
```typescript
// claudeService.ts
const generateBrief = async (profile: HVRProfile, threatData: ThreatData) => {
  const response = await fetch('https://api.anthropic.com/v1/messages', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'x-api-key': settings.claudeApiKey,
      'anthropic-version': '2023-06-01',
      'anthropic-dangerous-direct-browser-access': 'true'
    },
    body: JSON.stringify({
      model: settings.claudeModel,
      max_tokens: 4096,
      system: buildSentinelPrompt(profile),
      messages: [{ 
        role: 'user', 
        content: buildBriefRequest(threatData) 
      }]
    })
  });
};
```

---

## DATA PERSISTENCE

### IndexedDB Schema
```
Database: SentinelDB

Stores:
├── settings        # API keys, preferences (encrypted sensitive fields)
├── profile         # HVR profile data
├── countries       # 195 countries with threat scores
├── alerts          # Alert categories + enabled status
├── briefs          # Daily brief archive
├── watchlist       # Watched countries + reasons
├── exitPlans       # Checklist items + completion status
├── escapeRoutes    # Saved routes + waypoints
├── scanHistory     # Scan timestamps + results
└── threatScores    # Historical score data for trend analysis
```

### Seed Data Requirements
- 50+ countries with pre-calculated threat scores across 8 domains
- 80+ alert categories pre-populated (from list above)
- 15 Gauteng hijacking hotspots with GPS coordinates
- 3 exit plan templates (USA, Alternative, Emergency)
- 9 SA provinces with comparative safety data
- Pre-calculated escape routes from Johannesburg

---

## UI DESIGN DIRECTION

### Aesthetic: CIA Operations Center / Tactical Dark
- Primary background: `#0a0e17` (deep navy-black)
- Secondary background: `#111827` (dark slate)
- Panel background: `#1a1f2e` (slightly lighter slate)
- Accent color: `#00ff88` (matrix green) for safe indicators
- Warning: `#fbbf24` (amber)
- Danger: `#ef4444` (crimson)
- Critical: `#dc2626` (deep red)
- Text primary: `#e5e7eb` (light gray)
- Text secondary: `#9ca3af` (medium gray)
- Border: `#1e293b` (subtle dark border)
- Tab highlight: `rgba(0, 255, 136, 0.1)` (transparent green tint)
- Font: "JetBrains Mono" for data, "Inter" for UI text

### Globe Styling
- Dark satellite imagery base (Cesium dark theme)
- Country borders: subtle cyan `#0891b2` glow
- Atmosphere: subtle blue glow around Earth
- Stars background enabled
- Cloud layer optional

### Animations
- Tab transitions: 200ms ease-out fade
- Sidebar slide: 300ms cubic-bezier(0.4, 0, 0.2, 1)
- Globe rotation: smooth 60fps
- Threat bar fills: 500ms ease-out on load
- Alert notifications: slide-in from right with fade

---

## PREPPER & SOVEREIGNTY INTELLIGENCE SOURCES

The system should track and score these preparedness dimensions:

```
PERSONAL PREPAREDNESS:
- Food storage (30/60/90 day supply)
- Water purification capability
- Medical supplies and knowledge
- Self-defense capability and legal status
- Communications (ham radio, satellite phone)
- Energy independence (solar, generator, fuel)
- Financial resilience (cash, metals, crypto diversity)
- Skills diversity (first aid, navigation, mechanics)
- Community network strength
- Bug-out location identified and provisioned

SOVEREIGNTY THREATS TO MONITOR:
- CBDC rollout progress (country by country)
- Cash elimination timeline
- Digital ID mandate progress
- Social credit system indicators
- Property rights erosion
- Gun confiscation legislation
- Self-sufficiency restrictions
- Barter/alternative economy restrictions
- Off-grid living legality changes
- Mandatory vaccination / health mandates
- Freedom of movement restrictions
- Financial surveillance expansion
- Encryption ban attempts
- Right to repair restrictions
```

---

## DEVELOPMENT PHASES

### Phase 1: Foundation (Days 1-2)
```
Tasks:
1. Initialize Vite + React + TypeScript project
2. Install dependencies: cesium, zustand, tailwindcss, dexie (IndexedDB)
3. Create all TypeScript interfaces (types/sentinel.ts)
4. Build IndexedDB service with seed data
5. Implement sound manager (Web Audio API)
6. Create base layout: Header, TabBar, StatusBar
7. Implement tab navigation with click sounds
```

### Phase 2: Globe & Threat Engine (Days 3-4)
```
Tasks:
1. Integrate CesiumJS with dark theme
2. Implement country overlays with threat colors
3. Build continent fly-to buttons
4. Create ThreatSidebar with gradient bars
5. Implement heatmap switching
6. Build local threat scoring engine
7. Load 50+ countries with seed threat data
8. Implement click-to-select country interaction
```

### Phase 3: Brief & Agent (Days 5-6)
```
Tasks:
1. Build Claude API service with SENTINEL prompt
2. Implement Daily Brief generation and display
3. Build Brief History with search/filter
4. Create Agent chat panel with swoosh animations
5. Implement scheduled brief generation (8 AM)
6. Build import/export for briefs
7. Wire Agent to Claude API with brief context
```

### Phase 4: Alerts & Watchlist (Day 7)
```
Tasks:
1. Build Alert Dashboard with 80+ categories
2. Implement alert enable/disable toggles
3. Create severity bar components
4. Build Watchlist panel with Add Country modal
5. Implement country search dropdown
6. Create watchlist reason tracking
```

### Phase 5: Exit Plans & Escape Routes (Day 8)
```
Tasks:
1. Build Exit Plan Board with 3 plan templates
2. Implement checklist with completion tracking
3. Calculate and display readiness percentages
4. Integrate Google Maps for Escape Routes
5. Pre-calculate routes from Johannesburg
6. Build province risk comparison panel
7. Implement route risk overlay
```

### Phase 6: Profile & Settings (Day 9)
```
Tasks:
1. Build Profile form with pre-filled placeholders
2. Pre-populate skills matrix from HVR data
3. Build Settings panel with explicit SAVE buttons
4. Implement API key storage (encrypted)
5. Pre-populate Claude model dropdown
6. Build scan configuration controls
7. Implement data backup/restore
```

### Phase 7: Polish & Integration (Day 10)
```
Tasks:
1. End-to-end testing of all tabs
2. Sound effect tuning
3. Animation refinement
4. Error handling for API failures
5. Offline mode graceful degradation
6. Performance optimization (lazy loading tabs)
7. Final UI polish and responsive adjustments
```

---

## DEPENDENCY LIST

```json
{
  "dependencies": {
    "react": "^18.3.0",
    "react-dom": "^18.3.0",
    "cesium": "^1.120.0",
    "resium": "^1.18.0",
    "zustand": "^4.5.0",
    "dexie": "^4.0.0",
    "lucide-react": "^0.400.0",
    "date-fns": "^3.6.0",
    "tailwind-merge": "^2.3.0",
    "framer-motion": "^11.0.0",
    "@react-google-maps/api": "^2.19.0"
  },
  "devDependencies": {
    "typescript": "^5.5.0",
    "vite": "^5.4.0",
    "@types/react": "^18.3.0",
    "@vitejs/plugin-react": "^4.3.0",
    "tailwindcss": "^3.4.0",
    "autoprefixer": "^10.4.0",
    "postcss": "^8.4.0"
  }
}
```

---

## ENVIRONMENT VARIABLES

```env
# .env.local (never commit)
VITE_CLAUDE_API_KEY=sk-ant-your-key-here
VITE_CLAUDE_MODEL=claude-sonnet-4-5-20250929
VITE_GOOGLE_MAPS_API_KEY=AIza-your-key-here
VITE_CESIUM_ION_TOKEN=eyJ-your-token-here
```

---

## EXECUTION INSTRUCTIONS FOR CLAUDE CODE

```bash
# Step 1: Create project
npm create vite@latest sentinel -- --template react-ts
cd sentinel

# Step 2: Install all dependencies
npm install cesium resium zustand dexie lucide-react date-fns tailwind-merge framer-motion @react-google-maps/api

# Step 3: Install dev dependencies
npm install -D tailwindcss autoprefixer postcss @types/react

# Step 4: Initialize Tailwind
npx tailwindcss init -p

# Step 5: Copy Cesium static assets
# (resium handles this automatically with Vite plugin)

# Step 6: Create directory structure as defined above

# Step 7: Build following this document's specifications
# Start with Phase 1, proceed sequentially

# Step 8: Run development server
npm run dev
```

---

## CRITICAL RULES

1. **NO AUTOSAVE** — Every settings section has explicit SAVE button
2. **ALL SOUNDS** — Tab clicks, swoosh open/close, alert pings
3. **TRANSPARENT TABS** — Tab highlights must be semi-transparent
4. **PLACEHOLDER TEXT** — All input fields have dim placeholder guidance
5. **PRE-POPULATED DATA** — Skills, alert categories, exit plans, country data
6. **DARK THEME ONLY** — CIA operations center aesthetic throughout
7. **EXPLICIT SAVE** — Load saved settings on startup, save only on button click
8. **GLOBE INTERACTION** — Pan, tilt, zoom, rotate like Google Earth
9. **AGENT PANEL** — Swoosh open/close at bottom of Brief and History tabs
10. **IMPORT/EXPORT** — Briefs can be exported as JSON and imported back

---

## TOKEN MANAGEMENT WARNING

When developing in Claude chat sessions:
- Monitor prompt length actively
- At 80% token usage: create TasksToComplete.md with remaining work
- Finish all deliverables before reaching maximum length
- Never let a chat hit maximum length — files become undownloadable
- Warn the user proactively when approaching limits
