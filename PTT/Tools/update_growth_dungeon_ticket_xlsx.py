import os
import shutil
import tempfile
import zipfile
import xml.etree.ElementTree as ET
from pathlib import Path


NS = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
ET.register_namespace("", NS)


def qname(tag: str) -> str:
    return f"{{{NS}}}{tag}"


def load_shared_strings(xml_bytes: bytes):
    root = ET.fromstring(xml_bytes)
    strings = []
    index_map = {}
    for idx, si in enumerate(root.findall(qname("si"))):
        text = "".join(node.text or "" for node in si.iter(qname("t")))
        strings.append(text)
        index_map[text] = idx
    return root, strings, index_map


def get_or_add_shared_index(shared_root, strings, index_map, text: str) -> int:
    if text in index_map:
        return index_map[text]

    si = ET.SubElement(shared_root, qname("si"))
    t = ET.SubElement(si, qname("t"))
    t.text = text

    index = len(strings)
    strings.append(text)
    index_map[text] = index
    shared_root.set("count", str(len(strings)))
    shared_root.set("uniqueCount", str(len(strings)))
    return index


def ensure_shared_counts(shared_root, strings):
    shared_root.set("count", str(len(strings)))
    shared_root.set("uniqueCount", str(len(strings)))


def get_sheet_root(xml_bytes: bytes):
    return ET.fromstring(xml_bytes)


def get_sheet_data(sheet_root):
    return sheet_root.find(qname("sheetData"))


def get_existing_rows(sheet_data):
    rows_by_item_id = {}
    max_row = 1
    for row in sheet_data.findall(qname("row")):
        row_index = int(row.attrib.get("r", "1"))
        max_row = max(max_row, row_index)
        first_cell = row.find(qname("c"))
        if first_cell is None:
            continue
        value_node = first_cell.find(qname("v"))
        if value_node is None or not value_node.text:
            continue
        rows_by_item_id[value_node.text] = row
    return rows_by_item_id, max_row


def set_cell(row, cell_ref: str, value, shared=False):
    cell = None
    for current in row.findall(qname("c")):
        if current.attrib.get("r") == cell_ref:
            cell = current
            break

    if cell is None:
        cell = ET.SubElement(row, qname("c"), {"r": cell_ref})

    if shared:
        cell.set("t", "s")
    elif "t" in cell.attrib:
        del cell.attrib["t"]

    value_node = cell.find(qname("v"))
    if value_node is None:
        value_node = ET.SubElement(cell, qname("v"))
    value_node.text = str(value)


def write_dimension(sheet_root, last_column: str, max_row: int):
    dimension = sheet_root.find(qname("dimension"))
    if dimension is None:
        dimension = ET.Element(qname("dimension"))
        sheet_root.insert(0, dimension)
    dimension.set("ref", f"A1:{last_column}{max_row}")


def upsert_point_rows(xlsx_path: Path):
    point_rows = [
        ("1010", "DungeonWeaponTicket", "0", "110"),
        ("1011", "DungeonExperienceTicket", "0", "111"),
        ("1012", "DungeonEquipmentTicket", "0", "112"),
        ("1013", "DungeonTrainingTicket", "0", "113"),
        ("1014", "DungeonEnhanceTicket", "0", "114"),
    ]

    with zipfile.ZipFile(xlsx_path, "r") as zin:
        files = {name: zin.read(name) for name in zin.namelist()}

    shared_root, strings, index_map = load_shared_strings(files["xl/sharedStrings.xml"])
    sheet_root = get_sheet_root(files["xl/worksheets/sheet1.xml"])
    sheet_data = get_sheet_data(sheet_root)
    rows_by_item_id, max_row = get_existing_rows(sheet_data)

    for item_id, type_name, show_in_wallet, sort_order in point_rows:
        row = rows_by_item_id.get(item_id)
        if row is None:
            max_row += 1
            row = ET.SubElement(sheet_data, qname("row"), {"r": str(max_row)})
            rows_by_item_id[item_id] = row

        set_cell(row, f"A{row.attrib['r']}", item_id)
        set_cell(row, f"B{row.attrib['r']}", get_or_add_shared_index(shared_root, strings, index_map, type_name), shared=True)
        set_cell(row, f"C{row.attrib['r']}", show_in_wallet)
        set_cell(row, f"D{row.attrib['r']}", sort_order)

    ensure_shared_counts(shared_root, strings)
    write_dimension(sheet_root, "D", max_row)

    files["xl/sharedStrings.xml"] = ET.tostring(shared_root, encoding="utf-8", xml_declaration=True)
    files["xl/worksheets/sheet1.xml"] = ET.tostring(sheet_root, encoding="utf-8", xml_declaration=True)
    rewrite_zip(xlsx_path, files)


def upsert_item_rows(xlsx_path: Path):
    item_rows = [
        ("1010", "Point", "Point", "1", "Common", "Max", "0"),
        ("1011", "Point", "Point", "1", "Common", "Max", "0"),
        ("1012", "Point", "Point", "1", "Common", "Max", "0"),
        ("1013", "Point", "Point", "1", "Common", "Max", "0"),
        ("1014", "Point", "Point", "1", "Common", "Max", "0"),
    ]

    with zipfile.ZipFile(xlsx_path, "r") as zin:
        files = {name: zin.read(name) for name in zin.namelist()}

    shared_root, strings, index_map = load_shared_strings(files["xl/sharedStrings.xml"])
    sheet_root = get_sheet_root(files["xl/worksheets/sheet1.xml"])
    sheet_data = get_sheet_data(sheet_root)
    rows_by_item_id, max_row = get_existing_rows(sheet_data)

    for item_id, storage_type, item_type, is_stack, rarity, tier, unlock_condition in item_rows:
        row = rows_by_item_id.get(item_id)
        if row is None:
            max_row += 1
            row = ET.SubElement(sheet_data, qname("row"), {"r": str(max_row)})
            rows_by_item_id[item_id] = row

        set_cell(row, f"A{row.attrib['r']}", item_id)
        set_cell(row, f"B{row.attrib['r']}", get_or_add_shared_index(shared_root, strings, index_map, storage_type), shared=True)
        set_cell(row, f"C{row.attrib['r']}", get_or_add_shared_index(shared_root, strings, index_map, item_type), shared=True)
        set_cell(row, f"D{row.attrib['r']}", is_stack)
        set_cell(row, f"E{row.attrib['r']}", get_or_add_shared_index(shared_root, strings, index_map, rarity), shared=True)
        set_cell(row, f"F{row.attrib['r']}", get_or_add_shared_index(shared_root, strings, index_map, tier), shared=True)
        set_cell(row, f"G{row.attrib['r']}", unlock_condition)

    ensure_shared_counts(shared_root, strings)
    write_dimension(sheet_root, "G", max_row)

    files["xl/sharedStrings.xml"] = ET.tostring(shared_root, encoding="utf-8", xml_declaration=True)
    files["xl/worksheets/sheet1.xml"] = ET.tostring(sheet_root, encoding="utf-8", xml_declaration=True)
    rewrite_zip(xlsx_path, files)


def rewrite_zip(target_path: Path, files):
    fd, temp_path = tempfile.mkstemp(suffix=".xlsx")
    os.close(fd)
    Path(temp_path).unlink(missing_ok=True)
    try:
        with zipfile.ZipFile(temp_path, "w", zipfile.ZIP_DEFLATED) as zout:
            for name, data in files.items():
                zout.writestr(name, data)
        shutil.copyfile(temp_path, target_path)
    finally:
        Path(temp_path).unlink(missing_ok=True)


def main():
    data_root = Path(r"c:\GitFolder\ProjectTT\Data")
    upsert_point_rows(data_root / "Point.xlsx")
    upsert_item_rows(data_root / "Item.xlsx")
    print("Updated Point.xlsx and Item.xlsx with growth dungeon ticket rows.")


if __name__ == "__main__":
    main()
