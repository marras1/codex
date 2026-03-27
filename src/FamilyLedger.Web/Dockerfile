FROM nginx:1.27-alpine
COPY src/FamilyLedger.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY src/FamilyLedger.Web/index.html /usr/share/nginx/html/index.html
COPY src/FamilyLedger.Web/styles.css /usr/share/nginx/html/styles.css
COPY src/FamilyLedger.Web/app.js /usr/share/nginx/html/app.js
COPY src/FamilyLedger.Web/sw.js /usr/share/nginx/html/sw.js
COPY src/FamilyLedger.Web/manifest.webmanifest /usr/share/nginx/html/manifest.webmanifest
