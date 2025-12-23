function toggleSidebar(){
  document.body.classList.toggle('sidebar-open');
}

document.addEventListener('DOMContentLoaded', function(){
  if (typeof bootstrap === 'undefined' || !bootstrap.Modal) return;

  document.querySelectorAll('[data-modal-open=\"true\"]').forEach(function(el){
    var modal = bootstrap.Modal.getOrCreateInstance(el);
    if (!el.classList.contains('show')) {
      modal.show();
    }
  });

  function clearModalQuery(){
    var url = new URL(window.location.href);
    if (!url.searchParams.has('form') && !url.searchParams.has('id')) return;
    url.searchParams.delete('form');
    url.searchParams.delete('id');
    history.replaceState(null, document.title, url.pathname + url.search + url.hash);
  }

  document.querySelectorAll('.modal').forEach(function(el){
    el.addEventListener('hidden.bs.modal', function(){
      clearModalQuery();
    });
  });
});
