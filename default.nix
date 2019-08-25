with import <nixpkgs> {};

let
  paket-deps = import ./paket.nix;
  disinfo = stdenv.mkDerivation {
    name = "disinfo";
    src = ./.;

    buildInputs = [ mono dotnet-sdk paket-deps ];

    buildPhase = ''
      export HOME=$NIX_BUILD_TOP
      export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
      cp -r ${paket-deps}/.nuget $HOME
      cp -r ${paket-deps}/* ./
      ${dotnet-sdk}/bin/dotnet publish --no-dependencies --no-restore Disunity.Disinfo
    '';

    installPhase = ''
      mkdir -p $out
      cp Disunity.Disinfo/bin/Debug/netcoreapp2.2/publish/* $out
    '';
  };

in writeShellScriptBin "disinfo" ''
  ${dotnet-sdk}/bin/dotnet ${disinfo}/Disunity.Disinfo.dll
''
