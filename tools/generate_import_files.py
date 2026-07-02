#!/usr/bin/env python3
"""
Generuje przykładowe pliki CSV i XLSX do ręcznego wrzucenia do API importu.

Uruchomienie:
    python3 tools/generate_import_files.py
    python3 tools/generate_import_files.py --rows 50 --out output/

Wymagania:
    pip3 install openpyxl
"""

import argparse
import csv
import subprocess
import sys
from pathlib import Path


def ensure_openpyxl():
    try:
        import openpyxl
        return openpyxl
    except ImportError:
        print("Instaluję openpyxl...")
        subprocess.check_call([sys.executable, "-m", "pip", "install", "openpyxl", "-q", "--user"])
        import openpyxl
        return openpyxl


DEVICE_TYPES = ["LAPTOP", "PHONE", "TABLET", "DESKTOP", "MONITOR"]
SUBTYPES = ["", "GAMING", "BUSINESS", "ULTRABOOK", "CONVERTIBLE"]
MANUFACTURERS = ["", "APPLE", "DELL", "LENOVO", "HP", "SAMSUNG"]

COLUMNS = ["EanCode", "Name", "TypeCode", "SubtypeCode", "ManufacturerCode"]


def build_rows(count: int) -> list[dict]:
    rows = []
    for i in range(1, count + 1):
        rows.append({
            "EanCode": f"EAN-{i:06d}",
            "Name": f"Device {i}",
            "TypeCode": DEVICE_TYPES[i % len(DEVICE_TYPES)],
            "SubtypeCode": SUBTYPES[i % len(SUBTYPES)],
            "ManufacturerCode": MANUFACTURERS[i % len(MANUFACTURERS)],
        })
    return rows


def write_csv(rows: list[dict], path: Path) -> None:
    with open(path, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=COLUMNS)
        writer.writeheader()
        writer.writerows(rows)
    print(f"CSV  → {path}  ({len(rows)} wierszy)")


def write_xlsx(rows: list[dict], path: Path, openpyxl) -> None:
    wb = openpyxl.Workbook()
    ws = wb.active

    ws.append(COLUMNS)
    for row in rows:
        ws.append([row[col] for col in COLUMNS])

    for col_cells in ws.columns:
        max_len = max(len(str(cell.value or "")) for cell in col_cells)
        ws.column_dimensions[col_cells[0].column_letter].width = max_len + 2

    wb.save(path)
    print(f"XLSX → {path}  ({len(rows)} wierszy)")


def main():
    parser = argparse.ArgumentParser(description="Generuje pliki CSV/XLSX do importu urządzeń")
    parser.add_argument("--rows", type=int, default=10, help="Liczba wierszy (domyślnie: 10)")
    parser.add_argument("--out", type=str, default="tools/generated", help="Katalog wyjściowy (domyślnie: tools/generated)")
    parser.add_argument("--name", type=str, default="devices", help="Prefiks nazwy pliku (domyślnie: devices)")
    args = parser.parse_args()

    openpyxl = ensure_openpyxl()
    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)

    rows = build_rows(args.rows)
    write_csv(rows, out / f"{args.name}.csv")
    write_xlsx(rows, out / f"{args.name}.xlsx", openpyxl)


if __name__ == "__main__":
    main()
