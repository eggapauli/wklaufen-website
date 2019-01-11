docker run --rm `
    -p 8081:80 `
    -v $pwd\src\WkLaufen.Website.Server:/var/www/html `
    --name wklaufen `
    php:7.1-apache