# Command: generate-import

Generate sample CSV and XLSX device import files for manual API testing.

## Usage

```
/generate-import
/generate-import --rows 50
/generate-import --rows 100 --out output/ --name laptop-batch
```

## Arguments (all optional)

| Argument | Default | Description |
|----------|---------|-------------|
| `--rows` | 10 | Number of data rows |
| `--out` | `.` | Output directory |
| `--name` | `devices` | File name prefix |

## Steps

1. Run the generator script, passing any arguments the user provided:

```bash
python3 tools/generate_import_files.py [args...]
```

2. Confirm both files were created and show their paths.

3. Remind the user the files can be uploaded to:
   - `POST /api/import/device-imports` (multipart/form-data, field name: `file`)
