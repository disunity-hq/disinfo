with import <nixpkgs> {};

stdenv.mkDerivation {
  name = "disinfo-dependencies";
  src = ./.;

  # fixed-output derivation (hash must predetermined)
  outputHashAlgo = "sha256";
  outputHash = "010r0h5py08jrankg0hv58xzpq78yv4gb78d6bndy4797sg1r5ih";
  outputHashMode = "recursive";

  buildInputs = [ mono dotnet-sdk cacert git ];

  buildPhase = ''
    export HOME=$NIX_BUILD_TOP
    export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
    export SSL_CERT_FILE="${cacert}/etc/ssl/certs/ca-bundle.crt";
    ${mono}/bin/mono .paket/paket.exe install
    ${dotnet-sdk}/bin/dotnet restore
  '';

  installPhase = ''
    mkdir -p $out
    cp -r paket.lock paket-files packages .paket/Paket.Restore.targets $out
    cp -r --parents Disunity.Disinfo/obj/ $out
    cp -r $HOME/.nuget $out
  '';
}
