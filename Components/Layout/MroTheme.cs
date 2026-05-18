using MudBlazor;

namespace MroPlan.Components.Layout
{
    public static class MroTheme
    {
        public static MudTheme DefaultTheme { get; } = new()
        {
            PaletteDark = new PaletteDark()
            {
                Primary = "#FACC15",
                Secondary = "#3F3F46",
                Tertiary = "#475569",
                Background = "#050505",
                AppbarBackground = "#000000",
                AppbarText = "#FFFFFF",
                Surface = "#111113",
                TextPrimary = "#F4F4F5",
                TextSecondary = "#A1A1AA",
                Divider = "#27272A",
                ActionDefault = "#FACC15",
                DrawerBackground = "#0A0A0A",
                DrawerText = "#F4F4F5",
                Success = "#22C55E",
                Warning = "#F59E0B",
                Error = "#EF4444",
                Info = "#3B82F6"
            },
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "14px"
            },
            Typography = new Typography()
            {
                Default = new DefaultTypography()
                {
                    FontFamily = ["Plus Jakarta Sans", "Inter", "system-ui", "sans-serif"],
                    FontWeight = "400",
                    LineHeight = "1.6"
                },
                H3 = new H3Typography() { FontWeight = "800", LineHeight = "1.2" },
                H4 = new H4Typography() { FontWeight = "800", LineHeight = "1.2" },
                H5 = new H5Typography() { FontWeight = "700", LineHeight = "1.3" },
                H6 = new H6Typography() { FontWeight = "700", LineHeight = "1.3" },
                Body1 = new Body1Typography() { FontWeight = "400", LineHeight = "1.6" },
                Body2 = new Body2Typography() { FontWeight = "400", LineHeight = "1.5" },
                Button = new ButtonTypography() { FontWeight = "700", TextTransform = "none" }
            }
        };

        public static string CustomStyles => @"
            /* ═══════════════════════════════════════════════════════════════
               MROPRO INDUSTRIAL CYBER THEME — Ultra-Premium Design System
               ═══════════════════════════════════════════════════════════════ */

            /* === ROOT VARIABLES === */
            :root {
                --gold: #FACC15;
                --gold-dim: #D4A710;
                --gold-glow: rgba(250, 204, 21, 0.35);
                --gold-subtle: rgba(250, 204, 21, 0.08);
                --surface-0: #050505;
                --surface-1: #0A0A0C;
                --surface-2: #111113;
                --surface-3: #18181B;
                --surface-4: #27272A;
                --text-primary: #F4F4F5;
                --text-secondary: #A1A1AA;
                --text-muted: #71717A;
                --glass-bg: rgba(12, 12, 15, 0.72);
                --glass-border: rgba(255, 255, 255, 0.06);
                --radius: 14px;
                --transition: all 0.35s cubic-bezier(0.4, 0, 0.2, 1);
            }

            /* === GLOBAL BODY === */
            body {
                background-color: var(--surface-0) !important;
                font-family: 'Plus Jakarta Sans', 'Inter', system-ui, sans-serif;
                color: var(--text-primary);
                overflow-x: hidden;
            }

            /* Subtle grid pattern overlay */
            body::before {
                content: '';
                position: fixed;
                inset: 0;
                background-image: 
                    radial-gradient(circle at 25% 0%, rgba(250, 204, 21, 0.03) 0%, transparent 50%),
                    radial-gradient(circle at 75% 100%, rgba(250, 204, 21, 0.02) 0%, transparent 50%);
                pointer-events: none;
                z-index: 0;
            }

            /* Animated grid lines background */
            body::after {
                content: '';
                position: fixed;
                inset: 0;
                background-image: 
                    linear-gradient(rgba(250, 204, 21, 0.015) 1px, transparent 1px),
                    linear-gradient(90deg, rgba(250, 204, 21, 0.015) 1px, transparent 1px);
                background-size: 80px 80px;
                pointer-events: none;
                z-index: 0;
            }

            /* === SCROLLBAR === */
            ::-webkit-scrollbar {
                width: 6px;
                height: 6px;
            }
            ::-webkit-scrollbar-track {
                background: var(--surface-1);
            }
            ::-webkit-scrollbar-thumb {
                background: var(--surface-4);
                border-radius: 10px;
            }
            ::-webkit-scrollbar-thumb:hover {
                background: var(--gold-dim);
            }

            /* === GLASSMORPHISM NAV ISLAND === */
            .glass-nav {
                background: var(--glass-bg) !important;
                backdrop-filter: blur(30px) saturate(180%) !important;
                -webkit-backdrop-filter: blur(30px) saturate(180%) !important;
                border: 1px solid var(--glass-border) !important;
                box-shadow: 
                    0 25px 50px rgba(0, 0, 0, 0.6),
                    0 0 0 1px rgba(255, 255, 255, 0.03),
                    inset 0 1px 0 rgba(255, 255, 255, 0.04) !important;
                transition: var(--transition) !important;
            }
            .glass-nav::before {
                content: '';
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                height: 1px;
                background: linear-gradient(90deg, transparent, rgba(250, 204, 21, 0.2), transparent);
            }

            /* === PREMIUM TOP NAV === */
            .top-nav {
                display: flex;
                gap: 2px;
                align-items: center;
                background: rgba(255, 255, 255, 0.02);
                padding: 4px;
                border-radius: 12px;
                border: 1px solid rgba(255, 255, 255, 0.03);
            }
            .top-nav-link {
                color: var(--text-muted) !important;
                font-weight: 600 !important;
                padding: 8px 14px !important;
                border-radius: 10px !important;
                transition: var(--transition) !important;
                font-size: 0.78rem !important;
                letter-spacing: 0.8px;
                position: relative;
                white-space: nowrap !important;
            }
            .top-nav-link .mud-nav-link-text {
                white-space: nowrap !important;
            }
            .top-nav-link .mud-icon-root {
                margin-right: 4px !important;
            }
            .top-nav-link:hover {
                color: var(--text-primary) !important;
                background: rgba(255, 255, 255, 0.05) !important;
            }
            .top-nav .mud-nav-link.active {
                color: #000000 !important;
                background: linear-gradient(135deg, #FACC15 0%, #EAB308 50%, #D97706 100%) !important;
                box-shadow: 
                    0 0 30px rgba(250, 204, 21, 0.5),
                    0 4px 15px rgba(250, 204, 21, 0.35),
                    inset 0 1px 1px rgba(255, 255, 255, 0.4) !important;
                font-weight: 800 !important;
                transform: scale(1.05) translateY(-1px);
            }

            /* === PREMIUM CARDS (MUD-PAPER) === */
            .mud-paper {
                background: var(--surface-2) !important;
                border: 1px solid var(--glass-border) !important;
                box-shadow: 0 1px 3px rgba(0,0,0,0.3) !important;
                transition: var(--transition) !important;
                position: relative;
                overflow: hidden;
                border-radius: var(--radius) !important;
            }

            /* Shimmer sweep on hover */
            .mud-paper::before {
                content: '';
                position: absolute;
                top: 0;
                left: -100%;
                width: 60%;
                height: 100%;
                background: linear-gradient(
                    90deg,
                    transparent,
                    rgba(250, 204, 21, 0.03),
                    transparent
                );
                transform: skewX(-15deg);
                transition: left 0.6s ease;
                z-index: 1;
                pointer-events: none;
            }
            .mud-paper:hover::before {
                left: 130%;
            }
            .mud-paper:hover {
                border-color: rgba(250, 204, 21, 0.15) !important;
                box-shadow: 
                    0 20px 40px rgba(0, 0, 0, 0.4),
                    0 0 30px rgba(250, 204, 21, 0.05) !important;
                transform: translateY(-3px);
            }

            /* Top gold accent line on cards */
            .mud-paper::after {
                content: '';
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                height: 1px;
                background: linear-gradient(90deg, transparent 10%, rgba(250, 204, 21, 0.15), transparent 90%);
                pointer-events: none;
            }

            /* === KPI CARDS === */
            .kpi-card {
                position: relative;
                overflow: hidden;
                border: 1px solid var(--glass-border) !important;
                background: linear-gradient(145deg, var(--surface-2), var(--surface-3)) !important;
            }
            .kpi-card::before {
                content: '';
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                height: 3px;
                background: linear-gradient(90deg, var(--gold), var(--gold-dim));
                opacity: 0;
                transition: opacity 0.4s ease;
            }
            .kpi-card:hover::before {
                opacity: 1 !important;
            }
            .kpi-icon-wrap {
                width: 52px;
                height: 52px;
                border-radius: 14px;
                display: flex;
                align-items: center;
                justify-content: center;
                background: var(--gold-subtle);
                border: 1px solid rgba(250, 204, 21, 0.1);
            }
            .kpi-badge {
                background: var(--gold-subtle);
                color: var(--gold);
                padding: 4px 12px;
                border-radius: 20px;
                font-weight: 700;
                font-size: 0.7rem;
                letter-spacing: 1px;
                border: 1px solid rgba(250, 204, 21, 0.1);
            }
            .kpi-value {
                font-weight: 900 !important;
                font-size: 2.5rem !important;
                background: linear-gradient(135deg, #FFFFFF 0%, #A1A1AA 100%);
                -webkit-background-clip: text;
                -webkit-text-fill-color: transparent;
                line-height: 1 !important;
            }
            .kpi-label {
                color: var(--text-secondary) !important;
                font-weight: 500;
                font-size: 0.85rem;
                letter-spacing: 0.3px;
            }

            /* === SECTION HEADERS === */
            .section-header {
                display: flex;
                align-items: center;
                gap: 12px;
                margin-bottom: 24px;
            }
            .section-header::before {
                content: '';
                width: 4px;
                height: 24px;
                background: linear-gradient(180deg, var(--gold), transparent);
                border-radius: 4px;
            }

            /* === GLOW TEXT === */
            .glow-text {
                background: linear-gradient(135deg, #FFFFFF 0%, #D4D4D8 50%, #A1A1AA 100%);
                -webkit-background-clip: text;
                -webkit-text-fill-color: transparent;
            }

            /* === MILITARY BADGE === */
            .military-badge {
                background: linear-gradient(135deg, #FACC15 0%, #D97706 100%);
                color: black;
                padding: 6px 14px;
                border-radius: 8px;
                font-weight: 900;
                font-size: 0.72rem;
                letter-spacing: 1.2px;
                box-shadow: 
                    0 0 15px var(--gold-glow),
                    inset 0 1px 1px rgba(255, 255, 255, 0.4);
                text-transform: uppercase;
                position: relative;
            }

            /* === PAGE ENTRANCE ANIMATION === */
            .animate-fade-in {
                animation: pageEntrance 0.7s cubic-bezier(0.16, 1, 0.3, 1) forwards;
                opacity: 0;
            }
            @keyframes pageEntrance {
                from { 
                    opacity: 0; 
                    transform: translateY(25px); 
                    filter: blur(4px);
                }
                to { 
                    opacity: 1; 
                    transform: translateY(0); 
                    filter: blur(0px);
                }
            }

            /* === STAGGER ANIMATION FOR CHILDREN === */
            .stagger-children > * {
                opacity: 0;
                animation: staggerFade 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards;
            }
            .stagger-children > *:nth-child(1) { animation-delay: 0.05s; }
            .stagger-children > *:nth-child(2) { animation-delay: 0.10s; }
            .stagger-children > *:nth-child(3) { animation-delay: 0.15s; }
            .stagger-children > *:nth-child(4) { animation-delay: 0.20s; }
            .stagger-children > *:nth-child(5) { animation-delay: 0.25s; }
            .stagger-children > *:nth-child(6) { animation-delay: 0.30s; }
            .stagger-children > *:nth-child(7) { animation-delay: 0.35s; }
            .stagger-children > *:nth-child(8) { animation-delay: 0.40s; }

            @keyframes staggerFade {
                from {
                    opacity: 0;
                    transform: translateY(20px) scale(0.97);
                }
                to {
                    opacity: 1;
                    transform: translateY(0) scale(1);
                }
            }

            /* === TABLES === */
            .mud-table {
                background: transparent !important;
                border: none !important;
            }
            .mud-table .mud-table-head .mud-table-row .mud-table-cell {
                color: var(--text-muted) !important;
                font-weight: 700 !important;
                text-transform: uppercase;
                font-size: 0.72rem !important;
                letter-spacing: 1px;
                border-bottom: 1px solid var(--surface-4) !important;
                padding: 12px 16px !important;
                background: transparent !important;
            }
            .mud-table .mud-table-body .mud-table-row {
                transition: var(--transition) !important;
            }
            .mud-table .mud-table-body .mud-table-row:hover {
                background: var(--gold-subtle) !important;
            }
            .mud-table .mud-table-body .mud-table-row .mud-table-cell {
                border-bottom: 1px solid rgba(255, 255, 255, 0.03) !important;
                padding: 14px 16px !important;
                color: var(--text-primary) !important;
                font-size: 0.88rem;
            }

            /* === BUTTONS === */
            .mud-button-filled.mud-button-filled-primary {
                background: linear-gradient(135deg, var(--gold), var(--gold-dim)) !important;
                color: #000 !important;
                font-weight: 800 !important;
                box-shadow: 0 4px 15px rgba(250, 204, 21, 0.25) !important;
                border: none !important;
                transition: var(--transition) !important;
            }
            .mud-button-filled.mud-button-filled-primary:hover {
                box-shadow: 0 8px 25px rgba(250, 204, 21, 0.35) !important;
                transform: translateY(-1px);
            }

            /* === CHIPS === */
            .mud-chip {
                font-weight: 700 !important;
                font-size: 0.75rem !important;
                letter-spacing: 0.3px;
                border-radius: 8px !important;
            }

            /* === DIALOG === */
            .mud-dialog {
                background: var(--surface-2) !important;
                border: 1px solid var(--glass-border) !important;
                box-shadow: 0 30px 60px rgba(0, 0, 0, 0.7) !important;
                border-radius: 20px !important;
                backdrop-filter: blur(20px);
            }

            /* === DASHBOARD HERO === */
            .dash-hero {
                position: relative;
                padding: 32px 0;
            }
            .dash-hero-title {
                font-weight: 900 !important;
                font-size: 1.8rem !important;
                background: linear-gradient(135deg, #FFFFFF 0%, #A1A1AA 100%);
                -webkit-background-clip: text;
                -webkit-text-fill-color: transparent;
                letter-spacing: -0.5px;
            }
            .dash-hero-subtitle {
                color: var(--text-secondary) !important;
                font-weight: 500;
                font-size: 0.95rem;
                margin-top: 4px;
            }

            /* Status pulse dot */
            .status-dot {
                width: 8px;
                height: 8px;
                border-radius: 50%;
                background: #22C55E;
                display: inline-block;
                margin-right: 8px;
                animation: pulse-dot 2s ease-in-out infinite;
                box-shadow: 0 0 8px rgba(34, 197, 94, 0.4);
            }
            @keyframes pulse-dot {
                0%, 100% { opacity: 1; transform: scale(1); }
                50% { opacity: 0.6; transform: scale(0.85); }
            }

            /* === ANIMATED GRADIENT BORDER EFFECT === */
            .gradient-border {
                position: relative;
                border: none !important;
            }
            .gradient-border::before {
                content: '';
                position: absolute;
                inset: 0;
                border-radius: inherit;
                padding: 1px;
                background: linear-gradient(135deg, rgba(250, 204, 21, 0.3), transparent 40%, transparent 60%, rgba(250, 204, 21, 0.15));
                -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
                -webkit-mask-composite: xor;
                mask-composite: exclude;
                pointer-events: none;
            }

            /* === NOTIFICATION BADGE GLOW === */
            .mud-badge .mud-badge-content {
                box-shadow: 0 0 10px rgba(239, 68, 68, 0.5) !important;
                animation: badge-pulse 2s ease-in-out infinite;
            }
            @keyframes badge-pulse {
                0%, 100% { box-shadow: 0 0 10px rgba(239, 68, 68, 0.5); }
                50% { box-shadow: 0 0 20px rgba(239, 68, 68, 0.8); }
            }

            /* === CHART CONTAINER STYLING === */
            .chart-container .mud-paper {
                background: linear-gradient(145deg, var(--surface-2), rgba(17, 17, 19, 0.9)) !important;
            }

            /* === TOOLTIP OVERRIDE === */
            .mud-tooltip {
                background: var(--surface-3) !important;
                color: var(--text-primary) !important;
                border: 1px solid var(--glass-border) !important;
                box-shadow: 0 10px 25px rgba(0, 0, 0, 0.5) !important;
                border-radius: 10px !important;
                font-size: 0.85rem !important;
                font-weight: 500 !important;
            }

            /* === INPUT FIELDS === */
            .mud-input-control .mud-input {
                color: var(--text-primary) !important;
            }
            .mud-input-control .mud-input-underline::before {
                border-color: var(--surface-4) !important;
            }
            .mud-input-control .mud-input-underline::after {
                border-color: var(--gold) !important;
            }
            .mud-input-control .mud-input-label {
                color: var(--text-muted) !important;
            }
            .mud-input-control .mud-input-label.mud-input-label-animated.mud-input-label-inputcontrol {
                color: var(--text-muted) !important;
            }

            /* === SELECT / DROPDOWN === */
            .mud-popover .mud-list {
                background: var(--surface-2) !important;
                border: 1px solid var(--glass-border) !important;
            }
            .mud-list-item:hover {
                background: var(--gold-subtle) !important;
            }

            /* === LOADING SKELETON PULSE === */
            .skeleton-pulse {
                background: linear-gradient(90deg, var(--surface-3) 25%, var(--surface-4) 50%, var(--surface-3) 75%);
                background-size: 200% 100%;
                animation: skeleton-wave 1.5s ease-in-out infinite;
                border-radius: var(--radius);
            }
            @keyframes skeleton-wave {
                0% { background-position: 200% 0; }
                100% { background-position: -200% 0; }
            }

            /* === ICON CONTAINERS === */
            .icon-container {
                width: 44px;
                height: 44px;
                border-radius: 12px;
                display: flex;
                align-items: center;
                justify-content: center;
                transition: var(--transition);
            }
            .icon-container.gold {
                background: var(--gold-subtle);
                border: 1px solid rgba(250, 204, 21, 0.1);
                color: var(--gold);
            }
            .icon-container.success {
                background: rgba(34, 197, 94, 0.08);
                border: 1px solid rgba(34, 197, 94, 0.1);
                color: #22C55E;
            }
            .icon-container.danger {
                background: rgba(239, 68, 68, 0.08);
                border: 1px solid rgba(239, 68, 68, 0.1);
                color: #EF4444;
            }
            .icon-container.info {
                background: rgba(59, 130, 246, 0.08);
                border: 1px solid rgba(59, 130, 246, 0.1);
                color: #3B82F6;
            }

            /* === MUD-TAB STYLING === */
            .mud-tabs .mud-tab {
                color: var(--text-muted) !important;
                font-weight: 600 !important;
                transition: var(--transition) !important;
            }
            .mud-tabs .mud-tab.mud-tab-active {
                color: var(--gold) !important;
            }
            .mud-tabs .mud-tab-slider {
                background: var(--gold) !important;
            }

            /* === SNACKBAR === */
            .mud-snackbar {
                backdrop-filter: blur(20px) !important;
                border: 1px solid var(--glass-border) !important;
                border-radius: 14px !important;
            }

            /* ═══════════════════════════════════════════════════════════════
               PAGE-LEVEL DARK THEME OVERRIDES
               Forces ALL pages to follow the Industrial Cyber dark theme
               ═══════════════════════════════════════════════════════════════ */

            /* === NAV BAR FIX: Prevent text wrapping === */
            .top-nav-link .mud-nav-link-text {
                white-space: nowrap !important;
                font-size: 0.78rem !important;
            }

            /* === EXPANSION PANELS (İş Emri, Atölye pages) === */
            .mud-expand-panel {
                background: var(--surface-2) !important;
                border: 1px solid var(--glass-border) !important;
                border-radius: var(--radius) !important;
            }
            .mud-expand-panel .mud-expand-panel-header {
                background: transparent !important;
            }
            .mud-expand-panel .mud-expand-panel-content {
                background: var(--surface-2) !important;
            }
            .mud-expand-panel .mud-collapse-wrapper {
                border-top: 1px solid var(--glass-border) !important;
            }

            /* === FORCE ALL INLINE 'white' & light backgrounds to dark === */

            /* Personel KPI cards with inline background */
            .mud-paper[style*='background:#F8FAFC'],
            .mud-paper[style*='background:#F0FDF4'],
            .mud-paper[style*='background:#FEFCE8'],
            .mud-paper[style*='background:#F5F3FF'],
            .mud-paper[style*='background:#F0F9FF'],
            .mud-paper[style*='background:#FFF1F2'],
            .mud-paper[style*='background: #F8FAFC'],
            .mud-paper[style*='background: #FFFFFF'],
            .mud-paper[style*='background: white'] {
                background: var(--surface-2) !important;
                border-color: var(--glass-border) !important;
            }

            /* Override inline color #1B365D, #1e293b for dark mode */
            [style*='color:#1B365D'],
            [style*='color: #1B365D'],
            [style*='color:#1e293b'],
            [style*='color: #1e293b'] {
                color: var(--text-primary) !important;
            }

            /* Override inline color #64748B, #6B7280, #94a3b8 */
            [style*='color:#64748B'],
            [style*='color: #64748B'],
            [style*='color:#6B7280'],
            [style*='color: #6B7280'],
            [style*='color:#64748b'],
            [style*='color: #64748b'] {
                color: var(--text-secondary) !important;
            }

            /* Override inline border #E2E8F0, #e2e8f0 */
            [style*='border:1px solid #E2E8F0'],
            [style*='border: 1px solid #E2E8F0'],
            [style*='border:1px solid #e2e8f0'],
            [style*='border: 1px solid #e2e8f0'],
            .border-slate-200,
            .border {
                border-color: var(--glass-border) !important;
            }

            /* === YETKINLIK MATRISI PAGE === */
            .ym-kpi-card {
                background: var(--surface-2) !important;
                border: 1px solid var(--glass-border) !important;
                box-shadow: none !important;
            }
            .ym-kpi-card [style*='color:#1e293b'] {
                color: var(--text-primary) !important;
            }
            .ym-kpi-icon[style*='background:#eff6ff'] { background: rgba(59, 130, 246, 0.1) !important; }
            .ym-kpi-icon[style*='background:#f0fdf4'] { background: rgba(34, 197, 94, 0.1) !important; }
            .ym-kpi-icon[style*='background:#fef2f2'] { background: rgba(239, 68, 68, 0.1) !important; }

            .ym-drawer {
                background: var(--surface-2) !important;
                box-shadow: -8px 0 32px rgba(0,0,0,.5) !important;
            }
            .ym-drawer-header {
                border-bottom-color: var(--glass-border) !important;
            }
            .ym-personel-badge {
                background: var(--surface-3) !important;
                border-color: var(--glass-border) !important;
            }
            .ym-overlay {
                background: rgba(0,0,0,.5) !important;
            }

            /* Yetkinlik matris table dark mode */
            .ym-matrix thead { background: var(--surface-3) !important; }
            .ym-sticky-col { background: var(--surface-2) !important; border-right-color: var(--surface-4) !important; }
            .ym-matrix thead .ym-sticky-col { background: var(--surface-3) !important; }
            .ym-th-personel { background: var(--surface-3) !important; color: var(--text-secondary) !important; border-bottom-color: var(--surface-4) !important; }
            .ym-th-parca { background: var(--surface-3) !important; border-bottom-color: var(--surface-4) !important; border-left-color: var(--surface-4) !important; }
            .ym-pn-label { color: var(--text-primary) !important; }
            .ym-parca-adi { color: var(--text-muted) !important; }
            .ym-personel-name { background: var(--surface-2) !important; color: var(--text-primary) !important; border-bottom-color: var(--surface-4) !important; }
            .ym-cell { border-left-color: rgba(255,255,255,0.03) !important; border-bottom-color: rgba(255,255,255,0.03) !important; }
            .ym-cell-0 { background: var(--surface-3) !important; }
            .ym-cell-0 .ym-cell-val { color: var(--text-muted) !important; }
            .ym-matrix-wrap { border-color: var(--glass-border) !important; }
            .ym-personel-link { color: var(--text-primary) !important; }
            .ym-personel-link:hover { color: var(--gold) !important; }
            .ym-legend { border-top-color: var(--glass-border) !important; }
            .ym-legend-item { color: var(--text-secondary) !important; }

            /* Yetkinlik profil modal */
            .ym-profil-modal { background: var(--surface-2) !important; }
            .ym-profil-header { border-bottom-color: var(--glass-border) !important; }
            .ym-profil-stat { background: var(--surface-3) !important; border-color: var(--glass-border) !important; }
            .ym-profil-stat-val { color: var(--text-primary) !important; }
            .ym-profil-stat-lbl { color: var(--text-muted) !important; }

            /* Top 10 and risk rows */
            .ym-top-row[style*='background:#fffbeb'] { background: rgba(250, 204, 21, 0.08) !important; }
            .ym-top-row[style*='background:#f0f9ff'] { background: rgba(59, 130, 246, 0.06) !important; }
            .ym-top-row[style*='background:#fdf4ff'] { background: rgba(168, 85, 247, 0.06) !important; }
            .ym-top-row { color: var(--text-primary) !important; }
            .ym-risk-row { border-bottom-color: var(--glass-border) !important; }
            .ym-gap-row { border-bottom-color: var(--glass-border) !important; }
            .ym-cert-row { border-bottom-color: var(--glass-border) !important; }

            /* SVG radar text */
            .ym-profil-body text,
            svg text { fill: var(--text-secondary) !important; }
            .ym-matrix-empty { color: var(--text-muted) !important; }

            /* === PERSONEL YONETIMI PAGE === */
            .py-panel {
                background: var(--surface-2) !important;
                box-shadow: -4px 0 24px rgba(0,0,0,.5) !important;
            }
            .py-avatar {
                background: linear-gradient(135deg, var(--gold), #D97706) !important;
                color: black !important;
            }
            .py-stat-box {
                background: var(--surface-3) !important;
                border-color: var(--glass-border) !important;
            }
            .py-backdrop {
                background: rgba(0,0,0,.5) !important;
            }

            /* Search/filter inputs with inline white bg */
            [style*='background:white'],
            [style*='background: white'],
            [style*='background:#F9FAFB'],
            [style*='background: #F9FAFB'] {
                background: var(--surface-3) !important;
            }

            /* === İŞ EMRİ DAGITIM PAGE === */
            /* Vehicle cards */
            .vehicle-card {
                background: var(--surface-2) !important;
                border-color: var(--glass-border) !important;
            }
            .vehicle-card::before {
                background: var(--surface-4) !important;
            }
            .vehicle-card:hover {
                border-color: var(--gold) !important;
                box-shadow: 0 12px 20px -8px rgba(0, 0, 0, 0.4) !important;
            }
            .vehicle-card-active {
                border-color: var(--gold) !important;
                background: linear-gradient(to right, rgba(250, 204, 21, 0.06), var(--surface-2)) !important;
            }
            .vehicle-card-active::before {
                background: var(--gold) !important;
            }
            .vehicle-card-active .heli-icon-container {
                background: var(--gold) !important;
                color: black !important;
            }
            .vehicle-card-in-workshop {
                background: rgba(59, 130, 246, 0.05) !important;
                border-color: rgba(59, 130, 246, 0.15) !important;
            }
            .vehicle-card-in-workshop::before {
                background: #3b82f6 !important;
            }
            .vehicle-card-in-workshop:hover {
                background: rgba(59, 130, 246, 0.1) !important;
            }
            .heli-icon-container {
                background: var(--surface-3) !important;
                color: var(--text-secondary) !important;
            }

            /* İş Emri empty state placeholder */
            div[style*='background: #f8fafc'][class*='border-dashed'],
            div[style*='background:#f8fafc'][class*='border-dashed'] {
                background: var(--surface-2) !important;
                border-color: var(--glass-border) !important;
            }

            /* İş Emri right panel white backgrounds */
            div[style*='background: white'] {
                background: var(--surface-2) !important;
            }
            
            /* Hover filters (İş Emri mini KPIs) */
            .hover-filter[style*='background: #e0f2fe'] {
                background: rgba(59, 130, 246, 0.1) !important;
                border-color: rgba(59, 130, 246, 0.2) !important;
            }
            .hover-filter[style*='background: #dcfce7'] {
                background: rgba(34, 197, 94, 0.1) !important;
                border-color: rgba(34, 197, 94, 0.2) !important;
            }

            /* Modern table overrides */
            .modern-table .mud-table-head {
                background: var(--surface-3) !important;
                border-bottom: 2px solid var(--surface-4) !important;
            }
            .modern-table .mud-table-row:hover {
                background: var(--gold-subtle) !important;
            }

            /* Code tag overrides */
            code[style*='background: #f1f5f9'],
            code[style*='background:#f1f5f9'] {
                background: var(--surface-3) !important;
                color: var(--gold) !important;
            }

            /* === BAKIM KABUL PAGE === */
            /* Override the white form panel */
            .mud-paper[style*='background: #FFFFFF'],
            .mud-paper[style*='background:#FFFFFF'] {
                background: var(--surface-2) !important;
            }

            /* === ATOLYE OPERASYON PAGE === */
            .border-b {
                border-bottom-color: var(--glass-border) !important;
            }

            /* Scrollbar overrides for sub-containers */
            .vehicle-list-container::-webkit-scrollbar-thumb {
                background: var(--surface-4) !important;
            }
            .vehicle-list-container::-webkit-scrollbar-track {
                background: var(--surface-1) !important;
            }

            /* === MUD PROGRESS LINEAR DARK === */
            .mud-progress-linear {
                background: var(--surface-4) !important;
            }

            /* === UTILITIES === */
            .rounded-xxl { border-radius: 24px !important; }
            .animate-fade-in { animation: fadeIn 0.6s ease-out; }

            /* === PAGER === */
            .mud-table-pagination {
                color: var(--text-secondary) !important;
                border-top: 1px solid var(--glass-border) !important;
            }

            /* === BUTTON GROUP === */
            .mud-button-group .mud-button-outlined {
                border-color: var(--glass-border) !important;
                color: var(--text-secondary) !important;
            }
            .mud-button-group .mud-button-outlined:hover {
                background: var(--gold-subtle) !important;
                color: var(--gold) !important;
            }

            /* === DIVIDERS === */
            .mud-divider {
                border-color: var(--glass-border) !important;
            }

            /* === ATOLYE OPERASYON PAGE CARDS === */
            .atolye-card {
                background: var(--surface-2) !important;
                border-color: var(--glass-border) !important;
            }
            .atolye-card:hover {
                border-color: var(--gold) !important;
                box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.4) !important;
            }
            .atolye-card-active {
                border-color: var(--gold) !important;
                background: linear-gradient(135deg, var(--surface-2) 0%, rgba(250, 204, 21, 0.04) 100%) !important;
            }
            .atolye-icon-box {
                background: var(--surface-3) !important;
                color: var(--gold) !important;
            }
            .atolye-card:hover .atolye-icon-box {
                background: var(--gold) !important;
                color: black !important;
            }
            .active-indicator {
                background: var(--gold) !important;
            }

            /* Kanban columns */
            .kanban-column {
                background: var(--surface-1) !important;
                border-color: var(--glass-border) !important;
            }
            .active-column {
                background: rgba(59, 130, 246, 0.03) !important;
                border-color: rgba(59, 130, 246, 0.1) !important;
            }

            /* Task cards */
            .task-card {
                background: var(--surface-2) !important;
                border-color: var(--glass-border) !important;
            }
            .task-card:hover {
                border-color: var(--gold) !important;
                box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.3) !important;
            }
            .task-card.in-progress {
                border-left-color: #0284c7 !important;
            }
            .task-card.completed {
                background: rgba(34, 197, 94, 0.05) !important;
                border-color: rgba(34, 197, 94, 0.1) !important;
                opacity: 0.85;
            }

            /* Border-top override */
            .border-t {
                border-top-color: var(--glass-border) !important;
            }

            /* Stat group text override */
            .stat-group [style*='color:#1e293b'] {
                color: var(--text-primary) !important;
            }

            /* Personel icon color overrides in KPI cards */
            div[style*='background:#1B365D'],
            div[style*='background: #1B365D'] {
                background: linear-gradient(135deg, var(--gold), #D97706) !important;
            }
            div[style*='background:#16A34A'] { background: #16A34A !important; }
            div[style*='background:#CA8A04'] { background: #CA8A04 !important; }
            div[style*='background:#7C3AED'] { background: #7C3AED !important; }
            div[style*='background:#0EA5E9'] { background: #0EA5E9 !important; }
            div[style*='background:#E11D48'] { background: #E11D48 !important; }

            /* Personel page text colors override */
            [style*='color:#166534'] { color: #22C55E !important; }
            [style*='color:#854D0E'] { color: #FACC15 !important; }
            [style*='color:#5B21B6'] { color: #A78BFA !important; }
            [style*='color:#0369A1'] { color: #38BDF8 !important; }
            [style*='color:#9F1239'] { color: #FB7185 !important; }

            /* === INPUTS & FORMS === */
            .mud-input-outlined {
                background: rgba(255, 255, 255, 0.02) !important;
                border-color: var(--glass-border) !important;
                transition: var(--transition) !important;
            }
            .mud-input-outlined:hover {
                border-color: rgba(250, 204, 21, 0.3) !important;
                background: rgba(255, 255, 255, 0.04) !important;
            }
            .mud-input-outlined.mud-input-outlined-active {
                border-color: var(--gold) !important;
                background: rgba(255, 255, 255, 0.05) !important;
            }
            .mud-input-label {
                color: var(--text-secondary) !important;
                font-weight: 600 !important;
            }
            .mud-input-outlined-active .mud-input-label {
                color: var(--gold) !important;
            }
            .mud-select .mud-input-outlined-icon {
                color: var(--text-muted) !important;
            }
            .mud-select:hover .mud-input-outlined-icon {
                color: var(--gold) !important;
            }

            /* Dropdown lists */
            .mud-list {
                background: var(--surface-2) !important;
                padding: 6px !important;
            }
            .mud-list-item {
                border-radius: 8px !important;
                margin: 2px 0 !important;
                transition: var(--transition) !important;
            }
            .mud-list-item:hover {
                background: rgba(250, 204, 21, 0.1) !important;
                color: var(--gold) !important;
            }
            .mud-selected {
                background: var(--gold-subtle) !important;
                color: var(--gold) !important;
            }

            /* Scrollbars for list */
            .mud-list::-webkit-scrollbar { width: 6px; }
            .mud-list::-webkit-scrollbar-track { background: transparent; }
            .mud-list::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.1); border-radius: 10px; }
            .mud-list::-webkit-scrollbar-thumb:hover { background: rgba(255,255,255,0.2); }
        ";
    }
}
