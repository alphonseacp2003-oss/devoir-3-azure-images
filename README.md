
# Devoir 3 – Téléversement & redimensionnement d’images (Azure)
auteur : Romuald alphonse bampaly preira PRER18040300
## Objectif
- Téléverser une image via une Azure Function HTTP (POST)
- Stocker l’image dans le conteneur `images`
- Déclencher une Azure Function BlobTrigger pour créer une miniature dans `ireduites`
- Lister les miniatures via une Azure Function HTTP (GET)
- Afficher le tout dans une page Web statique

## Liens
- Site statique (Azure Static Website): https://alphonsestorage.z9.web.core.windows.net/
- Function App: https://alphonsefonction-hzeee3h3beada9gw.canadacentral-01.azurewebsites.net

## Endpoints
- POST `/api/Televerser` : téléverse une image (multipart/form-data)
- GET `/api/ListImages_reduites` : retourne la liste des URL d’images réduites

## Conteneurs Blob
- `images` : images originales
- `ireduites` : miniatures (256x256)

## Déploiement (CI/CD)
- `deploy-static.yml` : déploie `site/` dans le conteneur `$web`
- `deploy-functions.yml` : build + deploy des Azure Functions (.NET 8)
