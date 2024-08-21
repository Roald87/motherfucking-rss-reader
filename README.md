# Motherfucking RSS reader

A basic RSS reader, in the spirit of the motherfucking websites.

An rss reader without
- registration
- login
- platform dependence
- javascript

![screenshot of the RSS reader with three feeds from seth's blog, roaldin.ch and nature.com](website.png)

## Developers

To install the project
- `dotnet restore`

To run the few unit test
- `cd SimpleRssServer.Tests`
- `dotnet test`

Testing the whole thing
- `cd SimpleRssServer`
- `dotnet watch`

### Initial setup on Linux server

1. Create a new file for the service that starts the webserver
- `sudo nano /etc/systemd/system/mfrssreader-server.service`

2. Fill it with this content. The service runs as root, because then it can bind to port 80.

    ```
    [Unit]
    Description=HTTP server for motherfucking RSS reader
    After=network.target

    [Service]
    ExecStart=/var/www/rss/SimpleRssServer http://+:80/
    WorkingDirectory=/var/www/rss
    User=root
    Group=0
    Restart=always
    StandardOutput=append:/var/log/mfrssreader-server/rss.log
    StandardError=append:/var/log/mfrssreader-server/rss.err

    [Install]
    WantedBy=multi-user.target
    ```

3. Create the logging folders
- `sudo mkdir -p /var/log/mfrssreader-server`
- `sudo chown root:0 /var/log/mfrssreader-server`

1. Create the executable folder
- `sudo mkdir /var/www/rss`

4. After creating the service file, reload the systemd manager configuration to recognize the new service:
- `sudo systemctl daemon-reload`

Enable the service to start automatically at boot:
- `sudo systemctl enable mfrssreader-server.service`

Start the service immediately:
- `sudo systemctl start mfrssreader-server.service`

5. Check if the service is running:
- `sudo systemctl status mfrssreader-server.service`

6. Check the logs.
View the logs (stdout) using:
- `sudo tail -f /var/log/mfrssreader-server/rss.log`

View the error logs (stderr) using:
- `sudo tail -f /var/log/mfrssreader-server/rss.err`

### Deploying

To deploy, assuming the repo is cloned in `~/motherfucking-rss-reader/`
and the setup is done.
- `~/motherfucking-rss-reader/deploy.sh`
