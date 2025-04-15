{
  description = "The BibleBot infrastructure flake";
  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixpkgs-unstable";
    flake-parts.url = "github:hercules-ci/flake-parts";
    systems.url = "github:nix-systems/default";
    process-compose-flake.url = "github:Platonic-Systems/process-compose-flake";
    services-flake.url = "github:juspay/services-flake";
  };
  outputs =
    inputs:
    inputs.flake-parts.lib.mkFlake { inherit inputs; } {
      systems = import inputs.systems;
      imports = [
        inputs.process-compose-flake.flakeModule
      ];
      perSystem =
        {
          self',
          pkgs,
          config,
          lib,
          ...
        }:
        {
          # `process-compose.foo` will add a flake package output called "foo".
          # Therefore, this will add a default package that you can build using
          # `nix build` and run using `nix run`.
          process-compose."biblebot" =
            { config, ... }:
            let
              inherit (inputs.services-flake.lib) multiService;
            in
            {
              imports = [
                inputs.services-flake.processComposeModules.default
                (multiService ./src/BibleBot.Backend/backend.nix)
                (multiService ./src/BibleBot.AutomaticServices/autoserv.nix)
                (multiService ./src/BibleBot.Frontend/frontend.nix)
              ];

              services.nginx."nginx1" = {
                enable = true;
                httpConfig = ''
                  include ../../backend.conf
                '';
              };

              settings.processes = {
                init-nginx = {
                  command = pkgs.writeShellApplication {
                    runtimeInputs = [
                      pkgs.coreutils
                      pkgs.curl
                    ];
                    text = ''
                      PUBLIC_IPV4=$(curl -4 icanhazip.com);
                      cat > backend.conf << EOL
                      upstream backendcluster {
                        least_conn;
                        server localhost:5001;
                        server localhost:5002;
                        server localhost:5003;
                      }

                      server {
                        listen 5000;
                        server_name $PUBLIC_IPV4 localhost;

                        proxy_read_timeout 300;
                              proxy_connect_timeout 300;
                              proxy_send_timeout 300; 

                        location / {
                          proxy_pass http://backendcluster;
                        }

                        location /api {
                          proxy_pass http://backendcluster/api;	
                        }
                      }

                      # entries below are for pulsetic
                      server {
                        listen 5050;
                        server_name $PUBLIC_IPV4 localhost;

                        location / {
                          proxy_pass http://localhost:5001;
                        }

                        location ~* api {
                                deny all;
                        }
                      }

                      server {
                        listen 5051;
                        server_name $PUBLIC_IPV4 localhost;

                        location / {
                          proxy_pass http://localhost:5002;
                        }

                        location ~* api {
                          deny all;
                        }
                      }

                      server {
                        listen 5052;
                        server_name $PUBLIC_IPV4 localhost;

                        location / {
                          proxy_pass http://localhost:4999;
                        }

                        location ~* api {
                          deny all;
                        }
                      }

                      server {
                        listen 5053;
                        server_name $PUBLIC_IPV4 localhost;

                        location / {
                          proxy_pass http://localhost:5003;
                        }

                        location ~* api {
                          deny all;
                        }
                      }
                      EOL
                    '';
                    name = "init-nginx";
                  };
                };
              } // { "nginx1".depends_on."init-nginx".condition = "process_completed"; };

              services.backend = {
                backend1 = {
                  enable = true;
                };
                backend2 = {
                  enable = true;
                };
                backend3 = {
                  enable = true;
                };
              };

              services.autoserv."autoserv1".enable = true;
              services.frontend."frontend1".enable = true;
            };
        };
    };
}
