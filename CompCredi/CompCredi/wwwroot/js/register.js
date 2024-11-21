document.addEventListener("DOMContentLoaded", () => {
  const registerForm = document.getElementById("registerForm");

  if (registerForm) {
      registerForm.addEventListener("submit", async (event) => {
          event.preventDefault(); // Evita o reload padrão do formulário

          // Obtendo os valores dos campos
          const username = document.getElementById("username").value.trim();
          const email = document.getElementById("email").value.trim();
          const password = document.getElementById("password").value.trim();
          const confirmPassword = document.getElementById("confirmPassword").value.trim();

          // Validação de senhas
          if (password !== confirmPassword) {
              alert("As senhas não coincidem. Por favor, tente novamente.");
              return;
          }

          // Dados do usuário para enviar na requisição
          const userData = { username, email, password };

          try {
              // Fazendo a solicitação para registrar o usuário
              const response = await fetch(`${API_URL}/User/register`, {
                  method: "POST",
                  headers: {
                      "Content-Type": "application/json",
                  },
                  body: JSON.stringify(userData),
              });

              if (response.ok) {
                  alert("Registro realizado com sucesso!");
                  // Redireciona o usuário para a página de login
                  window.location.href = "login.html";
              } else {
                  // Trata os erros retornados pela API
                  const errorData = await response.json().catch(() => null); // Evita erros de JSON
                  alert(errorData?.message || "Erro ao registrar. Por favor, tente novamente.");
              }
          } catch (error) {
              console.error("Erro na solicitação:", error);
              alert("Erro inesperado. Por favor, tente novamente.");
          }
      });
  } else {
      console.error("Formulário de registro não encontrado no DOM.");
  }
});
