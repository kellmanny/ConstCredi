document.addEventListener("DOMContentLoaded", () => {
  loadCurrentUserInfo();
});

async function loadCurrentUserInfo() {
  const userId = getUserIdFromToken();

  if (!userId) {
    alert("Usuário não encontrado! Faça login novamente.");
    window.location.href = "login.html";
    return;
  }

  try {
    const response = await fetch(`${API_URL}/User/${userId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      const userData = await response.json();

      document.getElementById("editUsername").value = userData.username || "";
      document.getElementById("editEmail").value = userData.email || "";
      document.getElementById("editBiography").value = userData.biography || "";

      // Atualiza a imagem de perfil (adiciona um timestamp para evitar cache)
      document.getElementById("currentProfilePic").src = userData.profilePic
      ? `${API_URL.replace('/api', '')}${userData.profilePic}?t=${new Date().getTime()}`
      : "assets/profile-pic-placeholder.png"; // Mostra placeholder caso não exista imagem
    } else {
      console.error("Erro ao carregar perfil:", response.statusText);
      alert("Erro ao carregar informações do perfil.");
    }
  } catch (error) {
    console.error("Erro ao carregar perfil:", error);
    alert("Erro inesperado ao carregar informações do perfil.");
  }
}

document.getElementById("editProfileForm").addEventListener("submit", async (e) => {
  e.preventDefault();

  const username = document.getElementById("editUsername").value.trim();
  const email = document.getElementById("editEmail").value.trim();
  const password = document.getElementById("editPassword").value.trim();
  const biography = document.getElementById("editBiography").value.trim();
  const profilePic = document.getElementById("editProfilePic").files[0];

  try {
    // Atualizar foto de perfil
    if (profilePic) {
      const formData = new FormData();
      formData.append("file", profilePic);

      const picResponse = await fetch(`${API_URL}/User/upload-profile-pic`, {
        method: "POST",
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        body: formData,
      });

      if (!picResponse.ok) {
        const errorData = await picResponse.json();
        throw new Error(errorData.message || "Erro ao atualizar a foto de perfil.");
      } else {
        const data = await picResponse.json();
        console.log("Foto de perfil atualizada com sucesso.");

        // Atualiza a imagem exibida no frontend (adiciona timestamp para evitar cache do navegador)
        document.getElementById("currentProfilePic").src = `${API_URL}${data.profilePicUrl}?t=${new Date().getTime()}`;
      }
    }

    // Atualizar biografia
    if (biography) {
      const bioResponse = await fetch(`${API_URL}/User/update-bio`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({ biography }),
      });

      if (!bioResponse.ok) {
        const errorData = await bioResponse.json();
        throw new Error(errorData.message || "Erro ao atualizar a biografia.");
      } else {
        console.log("Biografia atualizada com sucesso.");
      }
    }

    // Atualizar outras informações
    const payload = {
      username: username || null,
      email: email || null,
      password: password || null,
    };

    const profileResponse = await fetch(`${API_URL}/User/update-profile`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
      body: JSON.stringify(payload),
    });

    if (!profileResponse.ok) {
      const errorData = await profileResponse.json();
      throw new Error(`Erro ao salvar alterações: ${errorData.message}`);
    }

    alert("Perfil atualizado com sucesso!");
    location.reload(); // Recarrega a página para refletir as mudanças
  } catch (error) {
    console.error("Erro ao salvar alterações:", error.message || error);
    alert("Erro inesperado ao salvar alterações.");
  }
});

function getUserIdFromToken() {
  const token = localStorage.getItem("token");
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.nameid || null;
  } catch (error) {
    console.error("Erro ao decodificar o token:", error);
    return null;
  }
}
