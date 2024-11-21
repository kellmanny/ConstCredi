document.addEventListener("DOMContentLoaded", function() {
  // Garantir que o DOM está completamente carregado
  const loginForm = document.getElementById('loginForm');

  if (loginForm) {
      loginForm.addEventListener('submit', async function(e) {
          e.preventDefault(); // Previne o comportamento padrão do formulário

          const username = document.getElementById('username').value;
          const password = document.getElementById('password').value;

          const response = await fetch(`${API_URL}/User/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ username, password })
        });
        

          if (response.ok) {
              const data = await response.json();
              localStorage.setItem("token", data.token); // Salva o token JWT no localStorage
              window.location.href = "index.html"; // Redireciona para a timeline
          } else {
              const data = await response.json();
              alert(data.message || "Erro ao realizar o login");
          }
      });
  } else {
      console.error('Formulário de login não encontrado!');
  }
});


