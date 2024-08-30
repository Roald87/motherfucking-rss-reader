git pull
dotnet publish
sudo systemctl stop mfrssreader-server.service
find /var/www/rss/* -not -name 'rss-cache' -not -path '/var/www/rss/rss-cache/*' -exec rm -rf {} +
cp -r SimpleRssServer/bin/Release/net8.0/publish/* /var/www/rss/
sudo systemctl start mfrssreader-server.service
