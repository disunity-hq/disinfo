with import <nixpkgs> {};

let
  disinfo = stdenv.mkDerivation {
    name = "disinfo";
    src = ./.;

        # fixed-output derivation (hash must predetermined)
    outputHashAlgo = "sha256";
    outputHash = "0b90afk0p6bcir8vpjdapqlxqyqd3065zwxgqsw5m7grki3qacsh";
    outputHashMode = "recursive";

    buildInputs = [ cacert git mono dotnet-sdk ];

    buildPhase = ''
      export HOME=$NIX_BUILD_TOP
      export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true      
      export SSL_CERT_FILE="${cacert}/etc/ssl/certs/ca-bundle.crt";
      ${mono}/bin/mono .paket/paket.exe install
      ${dotnet-sdk}/bin/dotnet publish Disunity.Disinfo
    '';

    installPhase = ''
      mkdir -p $out
      cp Disunity.Disinfo/bin/Debug/netcoreapp2.2/publish/* $out
    '';
  };

in writeShellScriptBin "disinfo" ''
  ${dotnet-sdk}/bin/dotnet ${disinfo}/Disunity.Disinfo.dll
''
