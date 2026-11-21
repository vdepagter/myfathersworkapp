"""Tile, Start menu and splash images for the MSIX package (tools/build-msix.ps1), from the app icon.

    python tools/msix-assets.py <icon-512.png> <Assets folder>
"""
import os
import sys

from PIL import Image

BACKGROUND = (17, 17, 17, 255)  # #111111, the app's theme colour

# name: (width, height, icon share of the shorter side; 0 = transparent background, icon fills it)
SIZES = {
    "StoreLogo.png": (50, 50, 0),
    "Square44x44Logo.png": (44, 44, 0),
    "Square44x44Logo.targetsize-44_altform-unplated.png": (44, 44, 0),
    "Square150x150Logo.png": (150, 150, 0.8),
    "Wide310x150Logo.png": (310, 150, 0.8),
    "SplashScreen.png": (620, 300, 0.8),
}


def main() -> int:
    icon = Image.open(sys.argv[1]).convert("RGBA")
    out = sys.argv[2]
    os.makedirs(out, exist_ok=True)
    for name, (w, h, share) in SIZES.items():
        if share == 0:
            img = icon.resize((w, h), Image.LANCZOS)
        else:
            img = Image.new("RGBA", (w, h), BACKGROUND)
            side = int(min(w, h) * share)
            img.alpha_composite(icon.resize((side, side), Image.LANCZOS), ((w - side) // 2, (h - side) // 2))
        img.save(os.path.join(out, name))
    return 0


if __name__ == "__main__":
    sys.exit(main())
