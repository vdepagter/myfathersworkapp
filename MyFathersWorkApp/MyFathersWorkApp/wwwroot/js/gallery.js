// Endings Gallery map (Pages/EndingsGallery.razor): draws the lines between the plaques of the endings map. Each
// .map-cell has data-i (its index), data-parent (the branch it hangs from, -1 for none) and data-lit (1 when reached).
// The lines are drawn only while the map is laid out as columns (wide screens); narrow screens hide the svg and use
// the left rails from the CSS instead. Redrawn on every Blazor render and when the map changes size.
(function () {
    "use strict";

    const SVG = "http://www.w3.org/2000/svg";

    function draw(map) {
        const svg = map.querySelector(".map-links");
        if (!svg) return;
        svg.replaceChildren();
        if (getComputedStyle(svg).display === "none") return;

        const box = map.getBoundingClientRect();
        svg.setAttribute("viewBox", `0 0 ${box.width} ${box.height}`);

        for (const cell of map.querySelectorAll(".map-cell")) {
            const parentId = cell.dataset.parent;
            if (parentId === undefined || parentId === "-1") continue;

            const parent = map.querySelector(`.map-cell[data-i="${parentId}"]`);
            const from   = parent?.querySelector(".plaque");
            const to     = cell.querySelector(".plaque");
            if (!from || !to) continue;

            const a   = from.getBoundingClientRect();
            const b   = to.getBoundingClientRect();
            const x1  = a.right - box.left - 6;
            const y1  = a.top + a.height / 2 - box.top;
            const x2  = b.left - box.left + 6;
            const y2  = b.top + b.height / 2 - box.top;
            const xm  = (x1 + x2) / 2;
            const lit = cell.dataset.lit === "1" && parent.dataset.lit === "1";

            const path = document.createElementNS(SVG, "path");
            path.setAttribute("d", `M${x1},${y1} C${xm},${y1} ${xm},${y2} ${x2},${y2}`);
            path.setAttribute("class", lit ? "lit" : "");
            svg.appendChild(path);
        }
    }

    window.mfwGallery = {
        links(id) {
            const map = document.getElementById(id);
            if (!map) return;
            draw(map);
            if (!map._mfwObserver) {
                map._mfwObserver = new ResizeObserver(() => draw(map));
                map._mfwObserver.observe(map);
            }
            // Fonts can arrive after the first render and change the plaque heights
            document.fonts?.ready.then(() => draw(map));
        }
    };
})();
