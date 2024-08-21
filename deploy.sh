git pull
dotnet publish
sudo systemctl stop mfrssreader-server.service
rm -rf /var/www/rss/*
cp -r SimpleRssServer/bin/Release/net8.0/publish/* /var/www/rss/
sudo systemctl start mfrssreader-server.service
