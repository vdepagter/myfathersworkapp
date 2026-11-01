// Screen transitions (docs/AUDIO_TRANSITIONS_MAP.md section 4), driven from Pages/Gameplay.razor (OnAfterRender, the same
// screen change detection that plays the transition sounds). CSS: "Screen transitions" in css/styles.css.
//   run(kind)    - the ink blot overlay #screen-transition (index.html, Unity's flip-book):
//                  "base"  - story window <-> hub: covers, holds 200 ms, then recedes to the top-right corner (500 ms).
//                            The hold must match _REVEAL_DELAY_MS in Pages/Gameplay.razor (the reveal sound starts with the recede).
//                  "popup" - a new GameplayPopup: recede only (500 ms).
//   enter(kind)  - entrance animations of the elements on the new screen:
//                  "popup" (GameplayPopup: setup and rules drop from the top, end of round / Generation zoom in on
//                  the parchment), "input" (GameplayInputPopup, zooms in), "rise" (a new story page slides up from the
//                  bottom, as in Unity), "same" (hub -> hub), "page" (setup / menu page after a navigation, from
//                  Layout/MainLayout.razor).
//   scrollTop()  - back to the top of the page when a new story window or a new round's hub is shown.
//   restoreScroll() - back to where the last click on the page was (not on a popup): the hub is not rendered while a
//                  popup covers it, so the page shrinks and the browser cuts the scroll position back.
// Both restart their animation (remove class, force reflow, add class) and clear it on animationend (or a timer). With
// prefers-reduced-motion the overlay is skipped; the entrances become a short opacity fade (CSS).
(function () {
    "use strict";

    // class and duration (ms, as in css/styles.css) per overlay kind
    const INK = { base: ["mfw-ink-base", 700], popup: ["mfw-ink-popup", 500] };

    // [selector, class, duration] per entrance kind
    const ENTRANCES = {
        popup: [[".popup-box.drop:not(.input-popup):not(.mirror-popup)", "mfw-enter-drop", 650],
                [".popup-box.zoom:not(.input-popup):not(.mirror-popup)", "mfw-enter-zoom", 600]],
        input: [[".popup-box.input-popup", "mfw-enter-zoom", 600]],
        same:  [[".gameplay-container", "mfw-enter-fade", 200]],
        rise:  [[".gameplay-container", "mfw-enter-rise", 500]],
        page:  [[".page-root > *", "mfw-enter-page", 350]]
    };

    const running = new WeakMap(); // element -> detach() of the animation that runs on it

    function reducedMotion() {
        return !!(window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches);
    }

    // Restarts the animation class on the element and removes it again on animationend. A timer backs that up: a hidden
    // tab does not fire animationend, and a class left behind would keep the overlay blocking input (or keep the
    // forwards fill overriding hover transforms). animationcancel is not used: restarting cancels the previous run, and
    // that event arrives after the new listener is attached.
    function play(el, cls, ms, allClasses) {
        const previous = running.get(el);
        if (previous) previous();

        allClasses.forEach(c => el.classList.remove(c));
        void el.offsetWidth; // force reflow so the animation starts again
        el.classList.add(cls);

        let timer = 0;
        const detach = () => {
            clearTimeout(timer);
            el.removeEventListener("animationend", onEnd);
            running.delete(el);
        };
        const clear = () => {
            detach();
            el.classList.remove(cls);
        };
        const onEnd = e => {
            if (e.target === el) clear(); // ignore animations of children
        };

        timer = setTimeout(clear, ms + 100);
        el.addEventListener("animationend", onEnd);
        running.set(el, detach);
    }

    function run(kind) {
        const ink = INK[kind];
        const el = document.getElementById("screen-transition");
        if (!ink || !el || reducedMotion()) return;

        play(el, ink[0], ink[1], Object.values(INK).map(i => i[0]));
    }

    // A page that fits the window would show a scroll bar for a moment while its content slides in from below, and the
    // scroll bar shifts the whole page sideways (classic scroll bars, e.g. Edge on Windows)
    let enteringTimer = 0;

    function hideScrollBarWhileEntering(ms) {
        const root = document.documentElement;
        if (!root.classList.contains("mfw-entering") && root.scrollHeight > root.clientHeight) return;
        root.classList.add("mfw-entering");
        clearTimeout(enteringTimer);
        enteringTimer = setTimeout(() => root.classList.remove("mfw-entering"), ms + 100);
    }

    function enter(kind) {
        const list = ENTRANCES[kind];
        if (!list) return;

        list.forEach(([selector, cls, ms]) => {
            const elements = document.querySelectorAll(selector);
            if (elements.length > 0 && !reducedMotion()) hideScrollBarWhileEntering(ms);
            elements.forEach(el => play(el, cls, ms, [cls]));
        });
    }

    // A new story page or round starts at the top (a long page left the next one scrolled down)
    function scrollTop() {
        window.scrollTo({ top: 0, left: 0, behavior: "instant" });
    }

    let clickedAt = 0; // scroll position of the last click in the page itself (a hub link), not on a popup

    document.addEventListener("click", e => {
        if (e.target instanceof Element && e.target.closest(".gameplay-container") && !document.querySelector(".popup-box")) {
            clickedAt = window.scrollY;
        }
    }, true);

    function restoreScroll() {
        window.scrollTo({ top: clickedAt, left: 0, behavior: "instant" });
    }

    // Scrolled page text must not show behind the header buttons (back, forward, pause, log book) and the audio toggles:
    // the frame is transparent there. While the page is scrolled, html.mfw-scrolled masks .gameplay-container above
    // --mfw-header-cut (the bottom of the buttons, in the container's own coordinates) with a short fade (Gameplay.razor.css).
    // Not at the top, so the Rankings podium (which fits the window, with a fixed shade inside the container) is never cut.
    let headerFrame = 0;

    function updateHeaderCut() {
        headerFrame = 0;
        const root      = document.documentElement;
        const container = document.querySelector(".gameplay-container");
        const header    = document.querySelector(".header-buttons");
        if (!container || !header || window.scrollY <= 0) {
            root.classList.remove("mfw-scrolled");
            return;
        }
        const cut = header.getBoundingClientRect().bottom + 6 - container.getBoundingClientRect().top;
        root.style.setProperty("--mfw-header-cut", Math.max(0, Math.round(cut)) + "px");
        root.classList.add("mfw-scrolled");
    }

    function queueHeaderCut() {
        if (!headerFrame) headerFrame = requestAnimationFrame(updateHeaderCut);
    }

    window.addEventListener("scroll", queueHeaderCut, { passive: true });
    window.addEventListener("resize", queueHeaderCut);
    setInterval(() => { if (!document.hidden) queueHeaderCut(); }, 1000);

    window.mfwTransitions = { run: run, enter: enter, scrollTop: scrollTop, restoreScroll: restoreScroll };
})();
