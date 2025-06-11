#!/bin/bash

certPassword="my-strong-cert-password321"
certPath="$HOME/.aspnet/https/adam-cert.pfx"

certDir=$(dirname "$certPath")
if [ ! -d "$certDir" ]; then
    mkdir -p "$certDir"
fi

dotnet dev-certs https --clean
dotnet dev-certs https --trust --password "$certPassword"
dotnet dev-certs https -ep "$certPath" -p "$certPassword"
