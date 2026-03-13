const _isOpen = ref(false)
const _movieId = ref<number | null>(null)

export const useMovieModal = () => ({
  isOpen: _isOpen,
  movieId: _movieId as Readonly<Ref<number | null>>,
  open(id: number) {
    _movieId.value = id
    _isOpen.value = true
  },
  close() {
    _isOpen.value = false
  },
})
