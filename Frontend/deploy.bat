@echo off
echo ============================================
echo 🚀 Iniciando build do projeto Vite...
echo ============================================
npm run build

echo ============================================
echo 🌍 Fazendo deploy para o GitHub Pages...
echo ============================================

cd dist
git init
git add .
git commit -m "Deploy frontend"
git branch -M gh-pages

:: ⚠️ Troque pelo SEU repositório:
git remote add origin https://github.com/Clinica-de-fisioterapia/backend-frontend.git

git push -f origin gh-pages

cd ..
echo ============================================
echo ✅ Deploy concluído com sucesso!
echo Acesse em: https://Clinica-de-fisioterapia.github.io/backend-frontend
echo ============================================
pause
