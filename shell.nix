{ pkgs ? import <nixpkgs> { } }:

pkgs.mkShell {
  nativeBuildInputs = with pkgs; [
    dotnetCorePackages.sdk_6_0
  ];
}
