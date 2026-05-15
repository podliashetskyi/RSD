# RSD

The RemSoft.Dev (RSD) marketing site + admin panel — a Blazor Server app on .NET 10 backed by Postgres, packaged with Docker Compose.

## Local development

```bash
cp .env.example .env       # fill in admin email/password, SMTP, signing keys
docker compose up -d --build
```

The site listens on `http://localhost:8082`. Admin lives under `/admin/login`.

## Stack reference

- ASP.NET Core 10 (Blazor Server)
- EF Core 10 + Npgsql
- Postgres 17 (Docker volume `pgdata`)
- Static assets under `wwwroot/`; uploaded media under `wwwroot/uploads/...` (Docker volume `uploads`)
- Output cache backed by Microsoft.AspNetCore.OutputCaching with tag-based invalidation

## Backup recipes

The site has two volumes that need backup: **`pgdata`** (Postgres database) and **`uploads`** (user-uploaded media on disk under `wwwroot/uploads/`). The application code, seed data, and build-time static assets are recreated from git + the Docker image, so they don't need backup.

### Backup

```bash
# Postgres dump (point-in-time snapshot of all rows)
docker exec rsd-postgres pg_dump -U rsd -d rsd -Fc \
  > "backups/rsd-$(date +%Y%m%d-%H%M%S).dump"

# Uploaded media (rsync the volume contents out of the container)
docker run --rm \
  -v rsd_uploads:/src:ro \
  -v "$(pwd)/backups":/dst \
  alpine sh -c "tar -czf /dst/uploads-$(date +%Y%m%d-%H%M%S).tar.gz -C /src ."
```

Run both on the same day; the Postgres dump references file paths under `uploads/...` and a mismatch leaves broken image links. The `Recount` button on `/admin/media` can resynchronise refcounts if the volumes drift.

### Restore

```bash
# Restore the database into a fresh container
docker exec -i rsd-postgres pg_restore -U rsd -d rsd --clean --if-exists \
  < backups/rsd-YYYYMMDD-HHMMSS.dump

# Restore the uploads volume
docker run --rm \
  -v rsd_uploads:/dst \
  -v "$(pwd)/backups":/src:ro \
  alpine sh -c "rm -rf /dst/* && tar -xzf /src/uploads-YYYYMMDD-HHMMSS.tar.gz -C /dst"
```

After restore, browse `/admin/media` and click **Recount** to reconcile `UploadedFile.RefCount` values with the restored entity data.

### Retention

There is no automated backup job in v1 — schedule the commands above on the host with `cron` or your platform's equivalent. Keep at least the last 14 daily dumps plus a weekly off-host copy for the database; uploads can be deduplicated weekly given their size.
