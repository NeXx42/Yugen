#!/bin/sh

if [ -z "$1" ]; then
  echo "Error: missing required argument"
  exit 1
fi

dotnet ef migrations add $1 --project Yugen.Data --startup-project Yugen.Api
dotnet ef database update --project Yugen.Data --startup-project Yugen.Api