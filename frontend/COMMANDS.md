# web3-001-ef-blockchain

## Commands:

```cmd
mkdir ./frontend
cd ./frontend
npm create vite@latest site -- --template react-ts
cd site
npm install

npm install tailwindcss @tailwindcss/vite
npm install lucide-react

npm run dev
```

```cmd
<!-- install -->
dotnet tool update -g docfx

<!-- start -->
cd ./frontend
mkdir docs
cd docs
docfx init -y

<!-- update -->
cd ./frontend/docs
docfx docfx.json
docfx serve _site
```

```cmd
cd ./frontend/site

tar --exclude='*/node_modules*' \
    --exclude='*/dist*' \
    -cvf build-site-ef-blockchain.tar .

caprover deploy \
  --caproverUrl https://caprover.edersonfernandes.tec.br \
  --appName ef-blockchain \
  --tarFile ./build-site-ef-blockchain.tar
```

```cmd
cd ./frontend/docs

tar --exclude='*/node_modules*' \
    -cvf build-docs-ef-blockchain.tar .

caprover deploy \
  --caproverUrl https://caprover.edersonfernandes.tec.br \
  --appName docs-ef-blockchain \
  --tarFile ./build-docs-ef-blockchain.tar
```

```cmd

```
