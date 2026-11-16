// Atmosphere effects from the Unity app (HANDOFF_ARCHIVE.md "Visual overhaul"): rain, fog, smoke and embers drawn on canvases with the
// original particle textures, a lightning flash, and the sparkle + expanding circle on every tap.
//   start(id, layers)  - fill canvas #id with particles. layers: [{ kind, count }], kind = rainBack | rainFront | fog |
//                        smoke | ember; count is for High quality (Medium draws ~40%, Low none).
//   stop(id)           - remove a canvas's particles (Blazor calls this when the page is disposed).
//   lightning(id)      - flash element #id now and then (High only).
//   quality / setQuality(q) - "high" | "medium" | "low", kept in localStorage "mfwGraphics". Default high, or low when
//                        the device asks for reduced motion. Low = no particles, no taps, no flashes; CSS freezes the
//                        rest through html[data-graphics="low"].
//   textSize / setTextSize(s) - story text scale 0.85-1.6 (Settings), localStorage "mfwTextSize", applied as the CSS
//                        variable --mfw-story-scale on <html> (css/theme.css).
// One requestAnimationFrame loop for all canvases; it skips hidden tabs and canvases that are not on screen.
(function () {
    "use strict";

    const KEY = "mfwGraphics";
    const SCALE = { high: 1, medium: 0.4, low: 0 };

    function readQuality() {
        try {
            const q = localStorage.getItem(KEY);
            if (q in SCALE) return q;
        } catch { /* storage blocked */ }
        return matchMedia("(prefers-reduced-motion: reduce)").matches ? "low" : "high";
    }

    let quality = readQuality();
    document.documentElement.dataset.graphics = quality;

    const TEXT_KEY = "mfwTextSize";
    function clampText(s) { return Math.min(1.6, Math.max(0.85, Number(s) || 1)); }
    let textSize = 1;
    try { textSize = clampText(localStorage.getItem(TEXT_KEY) ?? 1); } catch { /* storage blocked */ }
    document.documentElement.style.setProperty("--mfw-story-scale", textSize);

    function image(name) {
        const img = new Image();
        img.src = "images/ui/" + name + ".webp";
        return img;
    }

    const sprites = { cloud: image("fx-cloud"), ember: image("fx-ember") };
    const scenes = new Map(); // canvas id -> scene

    function spawn(kind, w, h, fresh) {
        const p = { kind };
        switch (kind) {
            case "rainBack":
            case "rainFront": {
                const front = kind === "rainFront";
                p.x   = Math.random() * w * 1.3 - w * 0.15;
                p.y   = fresh ? Math.random() * h : -40;
                p.len = front ? 18 + Math.random() * 22 : 8 + Math.random() * 10;
                p.v   = front ? 900 + Math.random() * 500 : 450 + Math.random() * 250;
                p.a   = front ? 0.22 + Math.random() * 0.25 : 0.12 + Math.random() * 0.12;
                p.w   = front ? 1.3 : 0.8;
                break;
            }
            case "fog":
            case "smoke": {
                const smoke = kind === "smoke";
                p.s  = (smoke ? 0.3 + Math.random() * 0.2 : 0.55 + Math.random() * 0.5) * h;
                p.x  = fresh ? Math.random() * w : -p.s;
                p.y  = smoke ? h * (0.8 + Math.random() * 0.15) : h * (0.62 + Math.random() * 0.3);
                p.v  = (smoke ? 6 : 10) + Math.random() * 14;
                p.a  = smoke ? 0.08 + Math.random() * 0.07 : 0.06 + Math.random() * 0.07;
                p.r  = Math.random() * 6.28;
                p.vr = (Math.random() - 0.5) * 0.03;
                break;
            }
            case "ember":
                p.x    = w * (0.15 + Math.random() * 0.7);
                p.y    = fresh ? h * (0.4 + Math.random() * 0.6) : h * (0.85 + Math.random() * 0.2);
                p.vx   = (Math.random() - 0.3) * 22;
                p.vy   = -(14 + Math.random() * 28);
                p.life = 0;
                p.max  = 5 + Math.random() * 6;
                p.s    = 3 + Math.random() * 4;
                p.ph   = Math.random() * 6;
                break;
        }
        return p;
    }

    function resize(scene) {
        const rect = scene.canvas.getBoundingClientRect();
        const dpr  = Math.min(window.devicePixelRatio || 1, 2);
        scene.w = rect.width;
        scene.h = rect.height;
        scene.canvas.width  = Math.round(rect.width * dpr);
        scene.canvas.height = Math.round(rect.height * dpr);
        scene.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    }

    function fill(scene) {
        scene.parts = [];
        const scale = SCALE[quality];
        for (const layer of scene.layers) {
            const count = Math.round(layer.count * scale);
            for (let i = 0; i < count; i++) scene.parts.push(spawn(layer.kind, scene.w, scene.h, true));
        }
        scene.ctx.clearRect(0, 0, scene.w, scene.h);
    }

    function step(scene, dt) {
        const { ctx, w, h, parts } = scene;
        const slant = 0.22;
        ctx.clearRect(0, 0, w, h);
        for (let i = 0; i < parts.length; i++) {
            const p = parts[i];
            if (p.kind === "rainBack" || p.kind === "rainFront") {
                p.y += p.v * dt;
                p.x += p.v * slant * dt;
                ctx.strokeStyle = `rgba(190,210,235,${p.a})`;
                ctx.lineWidth = p.w;
                ctx.beginPath();
                ctx.moveTo(p.x, p.y);
                ctx.lineTo(p.x - p.len * slant, p.y - p.len);
                ctx.stroke();
                if (p.y > h + 40) parts[i] = spawn(p.kind, w, h, false);
            } else if (p.kind === "fog" || p.kind === "smoke") {
                p.x += p.v * dt;
                p.r += p.vr * dt;
                ctx.save();
                ctx.globalAlpha = p.a;
                ctx.translate(p.x + p.s / 2, p.y);
                ctx.rotate(p.r);
                ctx.drawImage(sprites.cloud, -p.s / 2, -p.s / 2, p.s, p.s);
                ctx.restore();
                if (p.x > w + p.s) parts[i] = spawn(p.kind, w, h, false);
            } else {
                p.life += dt;
                p.x += (p.vx + Math.sin(p.life * 1.7 + p.ph) * 14) * dt;
                p.y += p.vy * dt;
                const t = p.life / p.max;
                ctx.globalAlpha = Math.max(0, Math.min(1, t * 4, (1 - t) * 2)) * (0.6 + 0.4 * Math.sin(p.life * 9 + p.ph));
                ctx.globalCompositeOperation = "lighter";
                ctx.drawImage(sprites.ember, p.x, p.y, p.s, p.s * 1.2);
                ctx.globalCompositeOperation = "source-over";
                ctx.globalAlpha = 1;
                if (p.life > p.max || p.y < -10) parts[i] = spawn("ember", w, h, false);
            }
        }
    }

    let last = performance.now();
    function frame(now) {
        const dt = Math.min(0.05, (now - last) / 1000);
        last = now;
        if (!document.hidden && quality !== "low") {
            for (const scene of scenes.values()) {
                if (scene.visible && scene.parts.length) step(scene, dt);
            }
        }
        requestAnimationFrame(frame);
    }
    requestAnimationFrame(frame);

    const observer = new IntersectionObserver(entries => {
        for (const e of entries) {
            const scene = scenes.get(e.target.id);
            if (scene) scene.visible = e.isIntersecting;
        }
    });

    window.addEventListener("resize", () => {
        for (const scene of scenes.values()) { resize(scene); fill(scene); }
    });

    // Tap: the Unity CircleClick ring plus a few sparks (WhiteParticle) where the screen was pressed
    const tapLayer = document.createElement("div");
    tapLayer.className = "mfw-tap-layer";
    tapLayer.setAttribute("aria-hidden", "true");
    document.addEventListener("DOMContentLoaded", () => document.body.appendChild(tapLayer));
    window.addEventListener("pointerdown", e => {
        if (quality === "low" || !tapLayer.isConnected) return;
        const tap = document.createElement("div");
        tap.className = "mfw-tap";
        tap.style.left = e.clientX + "px";
        tap.style.top  = e.clientY + "px";
        let html = '<div class="mfw-tap-ring"></div>';
        for (let i = 0; i < 7; i++) {
            const a = Math.random() * 6.28, d = 24 + Math.random() * 30;
            html += `<div class="mfw-tap-spark" style="--dx:${(Math.cos(a) * d).toFixed(1)}px;--dy:${(Math.sin(a) * d).toFixed(1)}px"></div>`;
        }
        tap.innerHTML = html;
        tapLayer.appendChild(tap);
        setTimeout(() => tap.remove(), 700);
    }, { passive: true });

    const flashes = new Map(); // element id -> timer

    window.mfwAtmosphere = {
        get quality() { return quality; },

        setQuality(q) {
            if (!(q in SCALE)) return;
            quality = q;
            document.documentElement.dataset.graphics = q;
            try { localStorage.setItem(KEY, q); } catch { /* storage blocked */ }
            for (const scene of scenes.values()) fill(scene);
        },

        get textSize() { return textSize; },
        getQuality: () => quality,
        getTextSize: () => textSize,

        setTextSize(s) {
            textSize = clampText(s);
            document.documentElement.style.setProperty("--mfw-story-scale", textSize);
            try { localStorage.setItem(TEXT_KEY, String(textSize)); } catch { /* storage blocked */ }
        },

        start(id, layers) {
            const canvas = document.getElementById(id);
            if (!canvas) return;
            this.stop(id);
            const scene = { canvas, ctx: canvas.getContext("2d"), layers, parts: [], visible: true, w: 0, h: 0 };
            scenes.set(id, scene);
            resize(scene);
            fill(scene);
            observer.observe(canvas);
        },

        stop(id) {
            const scene = scenes.get(id);
            if (scene) {
                observer.unobserve(scene.canvas);
                scenes.delete(id);
            }
            const timer = flashes.get(id);
            if (timer) {
                clearTimeout(timer);
                flashes.delete(id);
            }
        },

        lightning(id) {
            this.stop(id);
            const tick = () => {
                const el = document.getElementById(id);
                if (!el) { flashes.delete(id); return; }
                if (quality === "high" && !document.hidden) {
                    el.classList.remove("on");
                    void el.offsetWidth;
                    el.classList.add("on");
                }
                flashes.set(id, setTimeout(tick, 9000 + Math.random() * 12000));
            };
            flashes.set(id, setTimeout(tick, 4000 + Math.random() * 4000));
        }
    };
})();
